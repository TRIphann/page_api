using System.Text.Json;
using facbook_page_api.Models;

namespace facbook_page_api.Services
{
    public class FacebookGraphService : IFacebookGraphService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FacebookGraphService> _logger;
        private const string BaseUrl = "https://graph.facebook.com/v25.0";

        public FacebookGraphService(HttpClient httpClient, ILogger<FacebookGraphService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // GET /api/page/{pageId}
        public async Task<FacebookPage> GetPageInfoAsync(string pageId, string accessToken)
        {
            var url = $"{BaseUrl}/{pageId}?fields=id,name,about,fan_count,category,picture,followers_count&access_token={accessToken}";
            _logger.LogInformation("Getting page info for {PageId}", pageId);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook API error: {Content}", content);
                throw new Exception(content);
            }

            return JsonSerializer.Deserialize<FacebookPage>(content);
        }

        // GET /api/page/{pageId}/posts
        public async Task<FacebookPostsResponse> GetPagePostsAsync(string pageId, string accessToken)
        {
            var url = $"{BaseUrl}/{pageId}/posts?fields=id,message,created_time,full_picture,story&limit=25&access_token={accessToken}";
            _logger.LogInformation("Getting posts for page {PageId}", pageId);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook API error: {Content}", content);
                throw new Exception(content);
            }

            return JsonSerializer.Deserialize<FacebookPostsResponse>(content);
        }

        // POST /api/page/{pageId}/posts
        public async Task<FacebookActionResponse> CreatePostAsync(string pageId, string message, string accessToken)
        {
            var url = $"{BaseUrl}/{pageId}/feed";
            _logger.LogInformation("Creating post on page {PageId}", pageId);

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("message", message),
                new KeyValuePair<string, string>("access_token", accessToken)
            });

            var response = await _httpClient.PostAsync(url, formData);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook API error: {Content}", content);
                throw new Exception(content);
            }

            return JsonSerializer.Deserialize<FacebookActionResponse>(content);
        }

        // DELETE /api/page/post/{postId}
        public async Task<FacebookActionResponse> DeletePostAsync(string postId, string accessToken)
        {
            var url = $"{BaseUrl}/{postId}?access_token={accessToken}";
            _logger.LogInformation("Deleting post {PostId}", postId);

            var response = await _httpClient.DeleteAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook API error: {Content}", content);
                throw new Exception(content);
            }

            return JsonSerializer.Deserialize<FacebookActionResponse>(content);
        }

        // GET /api/page/post/{postId}/comments
        public async Task<FacebookCommentsResponse> GetPostCommentsAsync(string postId, string accessToken)
        {
            var url = $"{BaseUrl}/{postId}/comments?fields=id,message,from,created_time&access_token={accessToken}";
            _logger.LogInformation("Getting comments for post {PostId}", postId);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook API error: {Content}", content);
                throw new Exception(content);
            }

            return JsonSerializer.Deserialize<FacebookCommentsResponse>(content);
        }

        // GET /api/page/post/{postId}/likes
        public async Task<FacebookLikesResponse> GetPostLikesAsync(string postId, string accessToken)
        {
            var url = $"{BaseUrl}/{postId}/likes?summary=true&access_token={accessToken}";
            _logger.LogInformation("Getting likes for post {PostId}", postId);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook API error: {Content}", content);
                throw new Exception(content);
            }

            return JsonSerializer.Deserialize<FacebookLikesResponse>(content);
        }

        // GET /api/page/{pageId}/insights
        public async Task<FacebookInsightsResponse> GetPageInsightsAsync(string pageId, string accessToken)
        {
            var url = $"{BaseUrl}/{pageId}/insights?metric=page_follows&period=day&access_token={accessToken}";
            _logger.LogInformation("Getting insights for page {PageId}", pageId);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook API error: {Content}", content);
                throw new Exception(content);
            }

            return JsonSerializer.Deserialize<FacebookInsightsResponse>(content);
        }
    }
}
