using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
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
        private readonly IUserRepositories _userRepo;

        public EmailController(IMemoryCache memoryCache, IEmailRepositories emailRepo, ILogger<EmailController> logger, IUserRepositories userRepo)
        {
            _memoryCache = memoryCache;
            _emailRepo = emailRepo ?? throw new ArgumentNullException(nameof(emailRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        [HttpPost("sent-otp-email")]
        public async Task<ActionResult> SendOTPEmail([FromBody] VerifyEmailDTO email)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Post, "Đang gửi OTP tới {Email}", email?.Email);

                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(email?.Email))
                    {
                        _logger.LogWarning(CustomLogEvents.UserController_Put, "Email không hợp lệ");

                        var errorDetails = new ValidationProblemDetails(ModelState)
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                            Detail = "Email không hợp lệ"
                        };

                        return StatusCode(StatusCodes.Status400BadRequest, errorDetails.Detail);
                    }
                    var existingUser = await _userRepo.IsEmailExists(email.Email.ToLower());
                    if (existingUser)
                    {
                        _logger.LogWarning("Email đã tồn tại: {Email}", email.Email);

                        var errorDetails = new ValidationProblemDetails(ModelState)
                        {
                            Status = StatusCodes.Status409Conflict,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                            Detail = "Email đã được đăng ký"
                        };

                        return StatusCode(StatusCodes.Status409Conflict, errorDetails.Detail);
                    }

                    await _emailRepo.SendEmailAsync(email.Email, null, null, EmailType.Verification);

                    _logger.LogInformation(CustomLogEvents.UserController_Post, "OTP đã được gửi tới {Email}", email.Email);

                    return StatusCode(StatusCodes.Status200OK, "OTP đã được gửi");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Post, "Lỗi model state");

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };

                    return BadRequest(errorDetails.Errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi OTP tới {Email}", email?.Email);

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message
                });
            }
        }

        [HttpPost("resent-otp-email")]
        public async Task<ActionResult> ResendOTPEmail([FromBody] VerifyEmailDTO email)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Post, "Đang gửi OTP tới {Email}", email?.Email);

                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(email?.Email))
                    {
                        _logger.LogWarning(CustomLogEvents.UserController_Put, "Email không hợp lệ");

                        var errorDetails = new ValidationProblemDetails(ModelState)
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                            Detail = "Email không hợp lệ"
                        };

                        return StatusCode(StatusCodes.Status400BadRequest, errorDetails.Detail);
                    }
                    var existingUser = await _userRepo.IsEmailExists(email.Email.ToLower());
                    if (existingUser)
                    {
                        _logger.LogWarning("Email đã tồn tại: {Email}", email.Email);

                        var errorDetails = new ValidationProblemDetails(ModelState)
                        {
                            Status = StatusCodes.Status409Conflict,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                            Detail = "Email đã được đăng ký"
                        };

                        return StatusCode(StatusCodes.Status409Conflict, errorDetails.Detail);
                    }

                    await _emailRepo.SendEmailAsync(email.Email, null, null, EmailType.ResendOTP);

                    _logger.LogInformation(CustomLogEvents.UserController_Post, "OTP đã được gửi tới {Email}", email.Email);

                    return StatusCode(StatusCodes.Status200OK, "OTP đã được gửi");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Post, "Lỗi model state");

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };

                    return BadRequest(errorDetails.Errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi OTP tới {Email}", email?.Email);

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message
                });
            }
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult> VerifyEmail([FromBody] VerifyOtpDTO model)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Post, "Xác thực OTP {Email}", model?.Email);

                if (ModelState.IsValid)
                {
                    var cachedOtp = _memoryCache.Get<string>($"{model.Email}_otp");
                    if (cachedOtp == null)
                    {
                        _logger.LogWarning("OTP không tìm thấy cho {Email}", model.Email);

                        return NotFound(new ProblemDetails
                        {
                            Status = StatusCodes.Status404NotFound,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                            Detail = "OTP không tồn tại hoặc đã hết hạn"
                        });
                    }
                    if (model.Otp != cachedOtp)
                    {
                        _logger.LogWarning(CustomLogEvents.UserController_Put, "Lỗi OTP của {Email}", model.Email);

                        var errorDetails = new ValidationProblemDetails(ModelState)
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                            Detail = "OTP không đúng"
                        };

                        return StatusCode(StatusCodes.Status400BadRequest, errorDetails.Detail);
                    }
                    var isVerified = _memoryCache.Get<bool?>($"{model.Email}_verified");
                    if (isVerified == true)
                    {
                        _logger.LogWarning("Email đã được xác thực trước đó: {Email}", model.Email);

                        var errorDetails = new ValidationProblemDetails(ModelState)
                        {
                            Status = StatusCodes.Status409Conflict,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                            Detail = "Email đã được xác thực trước đó"
                        };

                        return StatusCode(StatusCodes.Status409Conflict, errorDetails.Detail);
                    }
                    _memoryCache.Set($"{model.Email}_verified", true, TimeSpan.FromMinutes(10));
                    _memoryCache.Remove($"{model.Email}_otp");

                    _logger.LogInformation(CustomLogEvents.UserController_Post, "Xác thực email thành công {Email}", model.Email);

                    return StatusCode(StatusCodes.Status200OK, "Xác thực email thành công");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Post, "Lỗi model state");

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };

                    return BadRequest(errorDetails.Errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {Email}", model?.Email);

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message
                });
            }
        }
    }
}
