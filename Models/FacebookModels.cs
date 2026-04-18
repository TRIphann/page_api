using System.Text.Json.Serialization;

namespace facbook_page_api.Models
{
    // ==================== PAGE ====================
    public class FacebookPage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("about")]
        public string About { get; set; }

        [JsonPropertyName("fan_count")]
        public int? FanCount { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("picture")]
        public FacebookPicture Picture { get; set; }

        [JsonPropertyName("followers_count")]
        public int? FollowersCount { get; set; }
    }

    public class FacebookPicture
    {
        [JsonPropertyName("data")]
        public FacebookPictureData Data { get; set; }
    }

    public class FacebookPictureData
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    // ==================== POSTS ====================
    public class FacebookPostsResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookPost> Data { get; set; }

        [JsonPropertyName("paging")]
        public FacebookPaging Paging { get; set; }
    }

    public class FacebookPost
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("created_time")]
        public string? CreatedTime { get; set; }

        [JsonPropertyName("full_picture")]
        public string FullPicture { get; set; }

        [JsonPropertyName("story")]
        public string Story { get; set; }
    }

    public class FacebookPaging
    {
        [JsonPropertyName("cursors")]
        public FacebookCursors Cursors { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }

        [JsonPropertyName("previous")]
        public string Previous { get; set; }
    }

    public class FacebookCursors
    {
        [JsonPropertyName("before")]
        public string Before { get; set; }

        [JsonPropertyName("after")]
        public string After { get; set; }
    }

    // ==================== COMMENTS ====================
    public class FacebookCommentsResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookComment> Data { get; set; }

        [JsonPropertyName("paging")]
        public FacebookPaging Paging { get; set; }
    }

    public class FacebookComment
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("from")]
        public FacebookUser From { get; set; }

        [JsonPropertyName("created_time")]
        public string? CreatedTime { get; set; }
    }

    public class FacebookUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    // ==================== LIKES ====================
    public class FacebookLikesResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookLike> Data { get; set; }

        [JsonPropertyName("summary")]
        public FacebookLikesSummary Summary { get; set; }

        [JsonPropertyName("paging")]
        public FacebookPaging Paging { get; set; }
    }

    public class FacebookLike
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class FacebookLikesSummary
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("can_like")]
        public bool? CanLike { get; set; }

        [JsonPropertyName("has_liked")]
        public bool? HasLiked { get; set; }
    }

    // ==================== INSIGHTS ====================
    public class FacebookInsightsResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookInsight> Data { get; set; }

        [JsonPropertyName("paging")]
        public FacebookPaging Paging { get; set; }
    }

    public class FacebookInsight
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("period")]
        public string Period { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("values")]
        public List<FacebookInsightValue> Values { get; set; }
    }

    public class FacebookInsightValue
    {
        [JsonPropertyName("value")]
        public object Value { get; set; }

        [JsonPropertyName("end_time")]
        public string? EndTime { get; set; }
    }

    // ==================== REQUESTS ====================
    public class CreatePostRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    // ==================== ACTION RESPONSE ====================
    public class FacebookActionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("success")]
        public bool? Success { get; set; }
    }

    // ==================== ERROR ====================
    public class FacebookErrorResponse
    {
        [JsonPropertyName("error")]
        public FacebookError Error { get; set; }
    }

    public class FacebookError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("fbtrace_id")]
        public string FbTraceId { get; set; }
    }
}
