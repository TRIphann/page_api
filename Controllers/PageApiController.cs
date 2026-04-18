using Microsoft.AspNetCore.Mvc;
using facbook_page_api.Models;
using facbook_page_api.Services;

namespace facbook_page_api.Controllers
{
    [ApiController]
    [Route("api/page")]
    public class PageApiController : ControllerBase
    {
        private readonly IFacebookGraphService _facebookService;
        private readonly ILogger<PageApiController> _logger;

        // ===== CỐ ĐỊNH Page ID và Access Token =====
        private const string PAGE_ID = "1046712038534955";
        private const string ACCESS_TOKEN = "EAAN7fFMrB7EBRGp6Lczfy3bBxf2VlfLyK7wcrrUKb8sI3DNythNXaL2ihrNne9BfdtbOa3yqwE9Uy8hpjgCh6Axqx9nAGLEmR3fkCH5mT5QNW9G2L1Qoz0jKllY7zOVq61afAB5ctDXB4AEbCzuTnxNQU2zFRZAuBGkhD4eN4tpxlpBceN3aPC9GpR35bCvpct46xTDvwym25XC2HisaUmFSyqOmBfIuTpQgZD";

        public PageApiController(
            IFacebookGraphService facebookService,
            ILogger<PageApiController> logger)
        {
            _facebookService = facebookService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/page/info - Lấy thông tin Page (đã cố định Page ID)
        /// </summary>
        [HttpGet("info")]
        public async Task<IActionResult> GetPageInfo()
        {
            try
            {
                var result = await _facebookService.GetPageInfoAsync(PAGE_ID, ACCESS_TOKEN);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page info for {PageId}", PAGE_ID);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/page/posts - Lấy danh sách bài viết của Page
        /// </summary>
        [HttpGet("posts")]
        public async Task<IActionResult> GetPagePosts()
        {
            try
            {
                var result = await _facebookService.GetPagePostsAsync(PAGE_ID, ACCESS_TOKEN);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts for {PageId}", PAGE_ID);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/page/posts - Đăng bài viết mới lên Page
        /// </summary>
        [HttpPost("posts")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Message))
                    return BadRequest(new { error = "Message is required." });

                var result = await _facebookService.CreatePostAsync(PAGE_ID, request.Message, ACCESS_TOKEN);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post on {PageId}", PAGE_ID);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/page/post/{postId} - Xóa bài viết
        /// </summary>
        [HttpDelete("post/{postId}")]
        public async Task<IActionResult> DeletePost(string postId)
        {
            try
            {
                var result = await _facebookService.DeletePostAsync(postId, ACCESS_TOKEN);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", postId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/page/post/{postId}/comments - Lấy comments của bài viết
        /// </summary>
        [HttpGet("post/{postId}/comments")]
        public async Task<IActionResult> GetPostComments(string postId)
        {
            try
            {
                var result = await _facebookService.GetPostCommentsAsync(postId, ACCESS_TOKEN);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for {PostId}", postId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/page/post/{postId}/likes - Lấy likes của bài viết
        /// </summary>
        [HttpGet("post/{postId}/likes")]
        public async Task<IActionResult> GetPostLikes(string postId)
        {
            try
            {
                var result = await _facebookService.GetPostLikesAsync(postId, ACCESS_TOKEN);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likes for {PostId}", postId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/page/insights - Lấy thống kê Page (đã cố định Page ID)
        /// </summary>
        [HttpGet("insights")]
        public async Task<IActionResult> GetPageInsights()
        {
            try
            {
                var result = await _facebookService.GetPageInsightsAsync(PAGE_ID, ACCESS_TOKEN);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting insights for {PageId}", PAGE_ID);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

