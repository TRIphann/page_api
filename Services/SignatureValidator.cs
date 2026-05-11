using System.Security.Cryptography;
using System.Text;

namespace facbook_page_api.Services
{
    /// <summary>
    /// Xác thực chữ ký X-Hub-Signature-256 từ Facebook.
    /// Facebook ký mỗi webhook request bằng HMAC-SHA256 với App Secret.
    /// </summary>
    public interface ISignatureValidator
    {
        bool ValidateSignature(string payload, string? signatureHeader);
    }

    public class SignatureValidator : ISignatureValidator
    {
        private readonly string _appSecret;
        private readonly ILogger<SignatureValidator> _logger;

        public SignatureValidator(IConfiguration configuration, ILogger<SignatureValidator> logger)
        {
            _logger = logger;
            _appSecret = configuration["Facebook:AppSecret"] ?? string.Empty;

            if (string.IsNullOrEmpty(_appSecret))
                _logger.LogWarning("⚠️ Facebook:AppSecret chưa cấu hình → Skip signature validation (dev mode)");
        }

        public bool ValidateSignature(string payload, string? signatureHeader)
        {
            // Dev mode: skip nếu chưa có AppSecret
            if (string.IsNullOrEmpty(_appSecret))
                return true;

            if (string.IsNullOrEmpty(signatureHeader))
            {
                _logger.LogWarning("⚠️ No X-Hub-Signature-256 header → allowing in dev mode");
                return true;
            }

            if (!signatureHeader.StartsWith("sha256="))
            {
                _logger.LogWarning("❌ Invalid X-Hub-Signature-256 format: {Sig}", signatureHeader ?? "(null)");
                return false;
            }

            var expectedSignature = signatureHeader["sha256=".Length..];

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_appSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            _logger.LogDebug("📐 SIG DEBUG | Received: {Recv} | Computed: {Comp} | AppSecret length: {Len}",
                expectedSignature.ToLowerInvariant(), computedSignature, _appSecret.Length);

            var isValid = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(expectedSignature.ToLowerInvariant()));

            if (!isValid)
                _logger.LogWarning("❌ Signature validation failed");
            else
                _logger.LogDebug("✅ Signature validated");

            return isValid;
        }
    }
}
