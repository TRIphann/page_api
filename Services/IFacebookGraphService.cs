using facbook_page_api.Models;

namespace facbook_page_api.Services
{
    public interface IFacebookGraphService
    {
        /// <summary>
        /// Lấy thông tin Page
        /// </summary>
        Task<FacebookPage> GetPageInfoAsync(string pageId, string accessToken);

        /// <summary>
        /// Lấy danh sách bài viết của Page
        /// </summary>
        Task<FacebookPostsResponse> GetPagePostsAsync(string pageId, string accessToken);

        /// <summary>
        /// Đăng bài viết mới lên Page
        /// </summary>
        Task<FacebookActionResponse> CreatePostAsync(string pageId, string message, string accessToken);

        /// <summary>
        /// Xóa bài viết
        /// </summary>
        Task<FacebookActionResponse> DeletePostAsync(string postId, string accessToken);

        /// <summary>
        /// Lấy danh sách comments của bài viết
        /// </summary>
        Task<FacebookCommentsResponse> GetPostCommentsAsync(string postId, string accessToken);

        /// <summary>
        /// Lấy danh sách likes của bài viết
        /// </summary>
        Task<FacebookLikesResponse> GetPostLikesAsync(string postId, string accessToken);

        /// <summary>
        /// Lấy thống kê Page
        /// </summary>
        Task<FacebookInsightsResponse> GetPageInsightsAsync(string pageId, string accessToken);
    }
}
