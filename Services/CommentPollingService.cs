using facbook_page_api.Models;
using facbook_page_api.Services;

namespace facbook_page_api.Services
{
    /// <summary>
    /// Background service polling Facebook mỗi 3 giây để phát hiện comment mới.
    /// Khi có comment mới → normalize → đẩy Kafka ngay lập tức.
    /// </summary>
    public class CommentPollingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CommentPollingService> _logger;
        private readonly HashSet<string> _processedCommentIds = new();
        private bool _isFirstRun = true;

        public CommentPollingService(
            IServiceProvider serviceProvider,
            IKafkaProducerService kafkaProducer,
            IConfiguration configuration,
            ILogger<CommentPollingService> logger)
        {
            _serviceProvider = serviceProvider;
            _kafkaProducer = kafkaProducer;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pageId = _configuration["Facebook:PageId"] ?? "";
            var accessToken = _configuration["Facebook:PageAccessToken"] ?? "";

            if (string.IsNullOrEmpty(pageId) || string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("CommentPolling: Missing PageId or AccessToken. Stopped.");
                return;
            }

            _logger.LogInformation("Comment Polling Service STARTED | Interval: 3s | Page: {PageId}", pageId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollNewComments(pageId, accessToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Polling error: {Msg}", ex.Message);
                }

                await Task.Delay(3000, stoppingToken);
            }
        }

        private async Task PollNewComments(string pageId, string accessToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var fbService = scope.ServiceProvider.GetRequiredService<IFacebookGraphService>();

            // Lấy bài viết
            FacebookPostsResponse? posts;
            try
            {
                posts = await fbService.GetPagePostsAsync(pageId, accessToken);
            }
            catch
            {
                return;
            }

            if (posts?.Data == null) return;

            int newCount = 0;
            foreach (var post in posts.Data.Take(5))
            {
                try
                {
                    var comments = await fbService.GetPostCommentsAsync(post.Id, accessToken);
                    if (comments?.Data == null) continue;

                    foreach (var comment in comments.Data)
                    {
                        if (_processedCommentIds.Contains(comment.Id)) continue;
                        _processedCommentIds.Add(comment.Id);

                        // Lần đầu chỉ ghi nhận comment cũ, không đẩy Kafka
                        if (_isFirstRun) continue;

                        // Comment mới → Normalize → Kafka
                        var evt = new NormalizedEvent
                        {
                            EventType = "comment",
                            Verb = "add",
                            PageId = pageId,
                            ObjectId = comment.Id,
                            PostId = post.Id,
                            Content = comment.Message,
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            ReceivedAt = DateTimeOffset.UtcNow.ToString("o"),
                            Sender = comment.From != null
                                ? new SenderInfo { Id = comment.From.Id, Name = comment.From.Name }
                                : null,
                            Metadata = new Dictionary<string, object>
                            {
                                ["source"] = "polling",
                                ["field"] = "feed",
                                ["reaction_type"] = ""
                            }
                        };

                        if (await _kafkaProducer.PublishEventAsync(evt))
                        {
                            newCount++;
                            _logger.LogInformation(
                                "NEW COMMENT >>> Kafka | From: {From} | Content: \"{Msg}\" | Post: {Post}",
                                comment.From?.Name ?? "?", comment.Message, post.Id);
                        }
                    }
                }
                catch { /* skip post */ }
            }

            if (_isFirstRun)
            {
                _logger.LogInformation("Loaded {Count} existing comments. Now watching for NEW comments...",
                    _processedCommentIds.Count);
                _isFirstRun = false;
            }
        }
    }
}
