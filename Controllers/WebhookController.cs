using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using facbook_page_api.Models;
using facbook_page_api.Services;

namespace facbook_page_api.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ISignatureValidator _signatureValidator;
        private readonly IEventNormalizerService _normalizer;
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly IFacebookGraphService _facebookService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhookController> _logger;
        private readonly string _verifyToken;

        public WebhookController(
            ISignatureValidator signatureValidator,
            IEventNormalizerService normalizer,
            IKafkaProducerService kafkaProducer,
            IFacebookGraphService facebookService,
            IConfiguration configuration,
            ILogger<WebhookController> logger)
        {
            _signatureValidator = signatureValidator;
            _normalizer = normalizer;
            _kafkaProducer = kafkaProducer;
            _facebookService = facebookService;
            _configuration = configuration;
            _logger = logger;
            _verifyToken = configuration["Facebook:VerifyToken"] ?? "my_verify_token";
        }

        /// <summary>Facebook Webhook Verification</summary>
        [HttpGet]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string? mode,
            [FromQuery(Name = "hub.verify_token")] string? token,
            [FromQuery(Name = "hub.challenge")] string? challenge)
        {
            if (mode == "subscribe" && token == _verifyToken)
            {
                WebhookStatusService.SetRegistered(true);
                _logger.LogInformation("✅ Webhook verified | Mode: webhook");
                return Ok(challenge);
            }
            _logger.LogWarning("⚠️ Webhook verify failed | Token mismatch");
            return Forbid();
        }

        /// <summary>Nhận event realtime từ Facebook Webhook</summary>
        [HttpPost]
        public async Task<IActionResult> ReceiveEvent()
        {
            string rawBody;
            using (var reader = new StreamReader(Request.Body))
                rawBody = await reader.ReadToEndAsync();

            _logger.LogInformation("📩 Facebook Webhook POST received | Body length: {Len}", rawBody.Length);

            if (string.IsNullOrWhiteSpace(rawBody))
                return BadRequest(new { error = "Empty body" });

            var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            if (!_signatureValidator.ValidateSignature(rawBody, signature))
                return Unauthorized(new { error = "Invalid signature" });

            var payload = JsonSerializer.Deserialize<FacebookWebhookPayload>(rawBody);
            if (payload == null || payload.Object != "page")
            {
                _logger.LogWarning("⚠️ Ignored non-page webhook: {Obj}", payload?.Object);
                return Ok(new { status = "ignored" });
            }

            // Ghi nhận event webhook
            WebhookStatusService.RecordEvent();

            var events = _normalizer.Normalize(payload);
            int success = 0;
            foreach (var evt in events)
            {
                if (await _kafkaProducer.PublishEventAsync(evt))
                {
                    success++;
                    WebhookStatusService.RecordEvent(evt.EventType);
                    _logger.LogInformation(
                        "✅ Facebook → Kafka | Type: {Type} | From: {From} | Content: \"{Msg}\"",
                        evt.EventType, evt.Sender?.Name ?? "?",
                        evt.Content?.Length > 40 ? evt.Content[..40] + "..." : evt.Content);
                }
            }

            return Ok(new { status = "processed", published = success });
        }

        /// <summary>Lấy tất cả comments từ mọi bài viết trên Page</summary>
        [HttpGet("all-comments")]
        public async Task<IActionResult> GetAllComments()
        {
            var pageId = _configuration["Facebook:PageId"] ?? "";
            var token = _configuration["Facebook:PageAccessToken"] ?? "";

            try
            {
                var posts = await _facebookService.GetPagePostsAsync(pageId, token);
                if (posts?.Data == null) return Ok(new { comments = new List<object>() });

                var allComments = new List<object>();
                foreach (var post in posts.Data)
                {
                    try
                    {
                        var comments = await _facebookService.GetPostCommentsAsync(post.Id, token);
                        if (comments?.Data == null) continue;

                        foreach (var c in comments.Data)
                        {
                            allComments.Add(new
                            {
                                id = c.Id,
                                message = c.Message,
                                from_name = c.From?.Name,
                                from_id = c.From?.Id,
                                created_time = c.CreatedTime,
                                post_id = post.Id,
                                post_message = post.Message?.Length > 50 ? post.Message[..50] + "..." : post.Message
                            });
                        }
                    }
                    catch { }
                }

                return Ok(new { total = allComments.Count, comments = allComments });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Lấy tất cả Kafka events đã nhận</summary>
        [HttpGet("events")]
        public IActionResult GetEvents()
        {
            var events = KafkaConsumerService.GetAllEvents();
            return Ok(new { total = events.Count, events });
        }

        /// <summary>SSE stream - push events realtime tới browser</summary>
        [HttpGet("stream")]
        public async Task Stream(CancellationToken cancellationToken)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            var clientId = Guid.NewGuid().ToString();
            var channel = KafkaConsumerService.Subscribe(clientId);

            _logger.LogInformation("SSE client connected: {Id}", clientId);

            try
            {
                await foreach (var evt in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    var json = JsonSerializer.Serialize(evt);
                    await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                KafkaConsumerService.Unsubscribe(clientId);
                _logger.LogInformation("SSE client disconnected: {Id}", clientId);
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { service = "webhook", status = "healthy", kafka_topic = "raw_events" });
        }

        /// <summary>Trạng thái webhook: đang dùng polling hay webhook thật, có tunnel URL chưa</summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(WebhookStatusService.GetStatus());
        }
    }
}
