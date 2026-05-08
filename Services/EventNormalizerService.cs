using facbook_page_api.Models;

namespace facbook_page_api.Services
{
    /// <summary>
    /// Normalize các event từ Facebook webhook payload thành schema chuẩn NormalizedEvent.
    /// Comment và Message có cấu trúc khác nhau từ Facebook,
    /// sau khi normalize đều ra cùng một schema để xử lý.
    /// </summary>
    public interface IEventNormalizerService
    {
        List<NormalizedEvent> Normalize(FacebookWebhookPayload payload);
    }

    public class EventNormalizerService : IEventNormalizerService
    {
        private readonly ILogger<EventNormalizerService> _logger;

        public EventNormalizerService(ILogger<EventNormalizerService> logger)
        {
            _logger = logger;
        }

        public List<NormalizedEvent> Normalize(FacebookWebhookPayload payload)
        {
            var events = new List<NormalizedEvent>();

            if (payload.Entry == null || !payload.Entry.Any())
                return events;

            foreach (var entry in payload.Entry)
            {
                // Xử lý feed changes (comments, reactions, posts...)
                if (entry.Changes != null)
                {
                    foreach (var change in entry.Changes)
                    {
                        var normalized = NormalizeFeedChange(entry, change);
                        if (normalized != null)
                            events.Add(normalized);
                    }
                }

                // Xử lý messaging events (tin nhắn)
                if (entry.Messaging != null)
                {
                    foreach (var messaging in entry.Messaging)
                    {
                        var normalized = NormalizeMessaging(entry, messaging);
                        if (normalized != null)
                            events.Add(normalized);
                    }
                }
            }

            _logger.LogInformation("Normalized {Count} events from webhook payload", events.Count);
            return events;
        }

        private NormalizedEvent? NormalizeFeedChange(WebhookEntry entry, WebhookChange change)
        {
            if (change.Value == null) return null;

            var value = change.Value;
            var eventType = value.Item?.ToLowerInvariant() switch
            {
                "comment" => "comment",
                "post" => "post",
                "reaction" => "reaction",
                "share" => "share",
                "like" => "reaction",
                _ => value.Item ?? "unknown"
            };

            return new NormalizedEvent
            {
                EventType = eventType,
                Verb = value.Verb ?? "unknown",
                PageId = entry.Id,
                ObjectId = eventType == "comment"
                    ? (value.CommentId ?? value.PostId ?? "unknown")
                    : (value.PostId ?? "unknown"),
                PostId = value.PostId,
                ParentId = value.ParentId,
                Content = value.Message,
                Timestamp = value.CreatedTime ?? entry.Time,
                ReceivedAt = DateTimeOffset.UtcNow.ToString("o"),
                Sender = value.From != null
                    ? new SenderInfo { Id = value.From.Id, Name = value.From.Name }
                    : value.SenderId != null
                        ? new SenderInfo { Id = value.SenderId, Name = value.SenderName }
                        : null,
                Metadata = new Dictionary<string, object>
                {
                    ["source"] = "feed",
                    ["field"] = change.Field,
                    ["reaction_type"] = value.ReactionType ?? ""
                }
            };
        }

        private NormalizedEvent? NormalizeMessaging(WebhookEntry entry, WebhookMessaging messaging)
        {
            if (messaging.Message == null) return null;

            return new NormalizedEvent
            {
                EventType = "message",
                Verb = "add",
                PageId = entry.Id,
                ObjectId = messaging.Message.Mid ?? Guid.NewGuid().ToString(),
                Content = messaging.Message.Text,
                Timestamp = messaging.Timestamp,
                ReceivedAt = DateTimeOffset.UtcNow.ToString("o"),
                Sender = messaging.Sender != null
                    ? new SenderInfo { Id = messaging.Sender.Id }
                    : null,
                Metadata = new Dictionary<string, object>
                {
                    ["source"] = "messaging",
                    ["recipient_id"] = messaging.Recipient?.Id ?? ""
                }
            };
        }
    }
}
