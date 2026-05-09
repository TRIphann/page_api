using System.Text.Json.Serialization;

namespace facbook_page_api.Models
{
    // ==================== ENUMS ====================

    public enum SpamLevel
    {
        None = 0,
        Light = 1,      // co link, chua xac dinh ro
        Medium = 2,     // nhieu link, hoac repeat
        Severe = 3      // scam, bot ro ràng, link độc hại
    }

    public enum SpamReason
    {
        None = 0,
        HasUrl = 1,
        RepeatedContent = 2,
        RepeatedCount = 3,  // 3+ repeat trong 24h
        MaliciousLink = 4,
        BotBehavior = 5
    }

    public enum UserIntent
    {
        Unknown = 0,
        AskPrice = 1,          // hoi gia
        Complaint = 2,         // khieu nai / ho tro
        PositiveFeedback = 3,  // khen / tich cuc
        NegativeFeedback = 4,  // chê / tieu cuc
        Question = 5,          // hoi thong tin chung
        Order = 6,             // dat hang
        Shipping = 7,          // hoi van chuyen / giao hang
        General = 8            // binh luan chung
    }

    public enum Sentiment
    {
        Neutral = 0,
        Positive = 1,
        Negative = 2
    }

    public enum EventProcessingStatus
    {
        Received = 0,
        Classified = 1,
        Processed = 2,
        Replied = 3,
        Failed = 4,
        Blacklisted = 5,
        Hidden = 6,
        PendingReview = 7
    }

    public enum ActionType
    {
        None = 0,
        AutoReply = 1,
        HideComment = 2,
        AddToBlacklist = 3,
        FlagForReview = 4,
        BlockUser = 5
    }

    // ==================== SPAM DETECTION RESULT ====================

    public class SpamDetectionResult
    {
        [JsonPropertyName("is_spam")]
        public bool IsSpam { get; set; }

        [JsonPropertyName("level")]
        public SpamLevel Level { get; set; }

        [JsonPropertyName("reasons")]
        public List<SpamReason> Reasons { get; set; } = new();

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("details")]
        public string? Details { get; set; }
    }

    // ==================== AI CLASSIFICATION RESULT ====================

    public class AIClassificationResult
    {
        [JsonPropertyName("intent")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserIntent Intent { get; set; }

        [JsonPropertyName("sentiment")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Sentiment Sentiment { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("reasoning")]
        public string? Reasoning { get; set; }

        [JsonPropertyName("raw_response")]
        public string? RawResponse { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    // ==================== PROCESSING DECISION ====================

    public class ProcessingDecision
    {
        [JsonPropertyName("action")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActionType Action { get; set; }

        [JsonPropertyName("auto_reply_text")]
        public string? AutoReplyText { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }  // 1=cao nhat, 10=thap nhat

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = "";

        [JsonPropertyName("requires_manual_review")]
        public bool RequiresManualReview { get; set; }

        [JsonPropertyName("block_user")]
        public bool BlockUser { get; set; }
    }

    // ==================== PROCESSED EVENT (kết quả cuối cùng) ====================

    public class ProcessedEvent
    {
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = "";

        [JsonPropertyName("original_event")]
        public NormalizedEvent? OriginalEvent { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EventProcessingStatus Status { get; set; }

        [JsonPropertyName("spam_result")]
        public SpamDetectionResult? SpamResult { get; set; }

        [JsonPropertyName("ai_result")]
        public AIClassificationResult? AIResult { get; set; }

        [JsonPropertyName("decision")]
        public ProcessingDecision? Decision { get; set; }

        [JsonPropertyName("received_at")]
        public DateTime ReceivedAt { get; set; }

        [JsonPropertyName("classified_at")]
        public DateTime? ClassifiedAt { get; set; }

        [JsonPropertyName("processed_at")]
        public DateTime? ProcessedAt { get; set; }

        [JsonPropertyName("replied_at")]
        public DateTime? RepliedAt { get; set; }

        [JsonPropertyName("failed_at")]
        public DateTime? FailedAt { get; set; }

        [JsonPropertyName("failure_reason")]
        public string? FailureReason { get; set; }

        [JsonPropertyName("retry_count")]
        public int RetryCount { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    // ==================== BLACKLIST ENTRY ====================

    public class BlacklistEntry
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = "";

        [JsonPropertyName("user_name")]
        public string? UserName { get; set; }

        [JsonPropertyName("page_id")]
        public string PageId { get; set; } = "";

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = "";

        [JsonPropertyName("spam_reasons")]
        public List<SpamReason> SpamReasons { get; set; } = new();

        [JsonPropertyName("spam_count_24h")]
        public int SpamCount24h { get; set; }

        [JsonPropertyName("total_spam_count")]
        public int TotalSpamCount { get; set; }

        [JsonPropertyName("added_at")]
        public DateTime AddedAt { get; set; }

        [JsonPropertyName("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [JsonPropertyName("auto_reply_disabled")]
        public bool AutoReplyDisabled { get; set; } = true;
    }

    // ==================== REVIEW QUEUE ENTRY ====================

    public class ReviewQueueEntry
    {
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = "";

        [JsonPropertyName("comment_id")]
        public string CommentId { get; set; } = "";

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("user_name")]
        public string? UserName { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("spam_level")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SpamLevel SpamLevel { get; set; }

        [JsonPropertyName("ai_intent")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserIntent Intent { get; set; }

        [JsonPropertyName("ai_sentiment")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Sentiment Sentiment { get; set; }

        [JsonPropertyName("decision_reason")]
        public string Reason { get; set; } = "";

        [JsonPropertyName("added_at")]
        public DateTime AddedAt { get; set; }

        [JsonPropertyName("reviewed")]
        public bool Reviewed { get; set; }

        [JsonPropertyName("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [JsonPropertyName("review_action")]
        public string? ReviewAction { get; set; } // hide, delete, approve
    }

    // ==================== PROCESSING STATS ====================

    public class ProcessingStats
    {
        [JsonPropertyName("total_processed")]
        public int TotalProcessed { get; set; }

        [JsonPropertyName("by_status")]
        public Dictionary<string, int> ByStatus { get; set; } = new();

        [JsonPropertyName("by_action")]
        public Dictionary<string, int> ByAction { get; set; } = new();

        [JsonPropertyName("by_intent")]
        public Dictionary<string, int> ByIntent { get; set; } = new();

        [JsonPropertyName("by_sentiment")]
        public Dictionary<string, int> BySentiment { get; set; } = new();

        [JsonPropertyName("spam_detected")]
        public int SpamDetected { get; set; }

        [JsonPropertyName("blacklisted_users")]
        public int BlacklistedUsers { get; set; }

        [JsonPropertyName("review_queue_size")]
        public int ReviewQueueSize { get; set; }

        [JsonPropertyName("retry_count")]
        public int TotalRetries { get; set; }
    }
}
