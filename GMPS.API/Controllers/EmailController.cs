using GMPS.API.DTOs;
using GPMS.DOMAIN.Constants;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailRepositories _emailRepo;
        private readonly ILogger<EmailController> _logger;
        private readonly IMemoryCache _memoryCache;

        public EmailController(IMemoryCache memoryCache, IEmailRepositories emailRepo, ILogger<EmailController> logger)
        {
            _memoryCache = memoryCache;
            _emailRepo = emailRepo ?? throw new ArgumentNullException(nameof(emailRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("sent-otp-email")]
        public async Task<IActionResult> SendOTPEmail([FromBody] VerifyEmailDTO email)
        {
            if (string.IsNullOrEmpty(email?.Email))
            {
                _logger.LogWarning(CustomLogEvents.UserController_Put, "Invalid email");
                var errorDetails = new ValidationProblemDetails(ModelState);
                errorDetails.Status = StatusCodes.Status400BadRequest;
                errorDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                return BadRequest(errorDetails.Errors);
            }    

            await _emailRepo.SendEmailAsync(email.Email, null, null, EmailType.Verification);
            _logger.LogInformation(CustomLogEvents.UserController_Post, "OTP email sent to {Email}", email.Email);
            return StatusCode(StatusCodes.Status200OK, "OTP đã được gửi");
        }

        [HttpPost("verify-email")]
        public IActionResult VerifyEmail([FromBody] VerifyOtpDTO model)
        {
            var cachedOtp = _memoryCache.Get<string>($"{model.Email}_otp");

            if (cachedOtp == null)
            {
                _logger.LogWarning(CustomLogEvents.UserController_Put, "Invalid OTP");
                var errorDetails = new ValidationProblemDetails(ModelState);
                errorDetails.Status = StatusCodes.Status400BadRequest;
                errorDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                return BadRequest(errorDetails.Errors);
            }

            if (model.Otp != cachedOtp)
            {
                _logger.LogWarning(CustomLogEvents.UserController_Put, "Invalid OTP");
                var errorDetails = new ValidationProblemDetails(ModelState);
                errorDetails.Status = StatusCodes.Status400BadRequest;
                errorDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                return BadRequest(errorDetails.Errors);
            }
            _memoryCache.Set($"{model.Email}_verified", true, TimeSpan.FromMinutes(10));
            _memoryCache.Remove($"{model.Email}_otp");

            return StatusCode(StatusCodes.Status200OK, "Xác thực email thành công");
        }
    }
}
