using System.Text.Json.Serialization;

namespace facbook_page_api.Models
{
    // ==================== WEBHOOK PAYLOAD ====================

    /// <summary>
    /// Root payload Facebook gửi đến webhook endpoint
    /// </summary>
    public class FacebookWebhookPayload
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("entry")]
        public List<WebhookEntry> Entry { get; set; } = new();
    }

    /// <summary>
    /// Mỗi entry đại diện cho một Page có sự kiện
    /// </summary>
    public class WebhookEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("changes")]
        public List<WebhookChange>? Changes { get; set; }

        [JsonPropertyName("messaging")]
        public List<WebhookMessaging>? Messaging { get; set; }
    }

    /// <summary>
    /// Thay đổi từ feed (comments, posts, reactions...)
    /// </summary>
    public class WebhookChange
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public WebhookChangeValue? Value { get; set; }
    }

    /// <summary>
    /// Chi tiết giá trị thay đổi trong feed
    /// </summary>
    public class WebhookChangeValue
    {
        [JsonPropertyName("item")]
        public string? Item { get; set; }

        [JsonPropertyName("verb")]
        public string? Verb { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("from")]
        public WebhookFrom? From { get; set; }

        [JsonPropertyName("post_id")]
        public string? PostId { get; set; }

        [JsonPropertyName("comment_id")]
        public string? CommentId { get; set; }

        [JsonPropertyName("parent_id")]
        public string? ParentId { get; set; }

        [JsonPropertyName("created_time")]
        public long? CreatedTime { get; set; }

        [JsonPropertyName("sender_name")]
        public string? SenderName { get; set; }

        [JsonPropertyName("sender_id")]
        public string? SenderId { get; set; }

        [JsonPropertyName("reaction_type")]
        public string? ReactionType { get; set; }
    }

    /// <summary>
    /// Thông tin người gửi trong feed change
    /// </summary>
    public class WebhookFrom
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    /// <summary>
    /// Messaging event (tin nhắn)
    /// </summary>
    public class WebhookMessaging
    {
        [JsonPropertyName("sender")]
        public WebhookParticipant? Sender { get; set; }

        [JsonPropertyName("recipient")]
        public WebhookParticipant? Recipient { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("message")]
        public WebhookMessage? Message { get; set; }
    }

    public class WebhookParticipant
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class WebhookMessage
    {
        [JsonPropertyName("mid")]
        public string? Mid { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    // ==================== NORMALIZED EVENT ====================

    /// <summary>
    /// Schema chuẩn hóa cho mọi event (comment, message, reaction...)
    /// Sau khi normalize, mọi event đều tuân theo cấu trúc này
    /// trước khi đẩy vào Kafka topic raw_events.
    /// </summary>
    public class NormalizedEvent
    {
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("verb")]
        public string Verb { get; set; } = string.Empty;

        [JsonPropertyName("page_id")]
        public string PageId { get; set; } = string.Empty;

        [JsonPropertyName("object_id")]
        public string ObjectId { get; set; } = string.Empty;

        [JsonPropertyName("post_id")]
        public string? PostId { get; set; }

        [JsonPropertyName("parent_id")]
        public string? ParentId { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("sender")]
        public SenderInfo? Sender { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("received_at")]
        public string ReceivedAt { get; set; } = DateTimeOffset.UtcNow.ToString("o");

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class SenderInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
