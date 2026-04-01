using Azure.Messaging;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepositories _accountRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailRepositories _emailRepo;
        public AccountController(IAccountRepositories accountRepo, IConfiguration configuration, ILogger<AccountController> logger,
            IEmailRepositories emailRepo)
        {
            _accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
            _configuration = configuration;
            _logger = logger;
            _emailRepo = emailRepo;
        }
        //Information
        // Warning
        // Error
        // Critical

        [HttpPost("login")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult> Login([FromBody] LoginDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _accountRepo.Login(input.UserName!, input.Password!);
                    if (user is null)
                    {
                        return NotFound(new
                        {
                            Code = Message_Codes.AUTH_LOGIN_FAILED,
                            Message = Message_Contents.LOGIN_FAILED,
                        });
                    }
                    else
                    {
                        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"])), SecurityAlgorithms.HmacSha256);
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name, user.User.UserName));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.User.Id.ToString())); // Unique
                        foreach (var role in user.UserRole.Select(r => r.Name))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        // Tao Jwt Token
                        var jwtObject = new JwtSecurityToken(
                            issuer: _configuration["JWT:Issuer"],
                            audience: _configuration["JWT:Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddYears(10),
                            signingCredentials: signingCredentials);
                        // Ky token cuoi cung va gui tra ve cho user
                        var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtObject);
                        //Logging thông tin đăng nhập thành công
                        _logger.LogInformation(CustomLogEvents.AccountController_Post, $"Tài khoản ({user.User.UserName}) đã đăng nhập thành công");
                        return StatusCode(StatusCodes.Status200OK, jwtString);
                    }
                }
                else
                {
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    _logger.LogWarning(CustomLogEvents.AccountController_Post, ModelState.ToString());
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status =
                StatusCodes.Status401Unauthorized;
                exceptionDetails.Type =
                "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                return StatusCode(
                StatusCodes.Status401Unauthorized,
                exceptionDetails);
            }

        }

        [HttpPost("register")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult> Register(RegisterDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!input.RePassword.Equals(input.Password))
                    {
                        var details = new ValidationProblemDetails();
                        details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                        details.Status = StatusCodes.Status400BadRequest;
                        details.Errors.Add("RePassword", new string[] { "Mật khẩu không khớp" });
                        return new BadRequestObjectResult(details);
                    }
                    var newUser = new User();
                    newUser.UserName = input.UserName;
                    newUser.FullName = input.FullName;
                    newUser.PasswordHash = input.Password;
                    newUser.Email = input.Email;
                
                    var result = await _accountRepo.Register(newUser);

                    if (result.Status == GPMS.APPLICATION.Enum.RegisterStatus.Success)
                    {
                        //      _logger.LogInformation("User {UserName} ({email}) has been created.", newUser.UserName, newUser.Email);
                        return StatusCode(201, $"User '{newUser.UserName}' has been created");
                    }
                    else if (result.Status == GPMS.APPLICATION.Enum.RegisterStatus.Failed)
                    {
                        var details = new ValidationProblemDetails();
                        details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                        details.Status = StatusCodes.Status400BadRequest;
                        //  details.Detail = string.Join(",", result.Errors);
                        details.Errors = result.Errors;
                        return new BadRequestObjectResult(details);
                    }
                    return Ok();
                }
                else
                {
                    var details = new ValidationProblemDetails();
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    details.Errors.Add("RePassword", new string[] { "Mật khẩu không khớp" });
                    return new BadRequestObjectResult(details);
                }
            }
            catch (DbUpdateException e)
            {
                var details = new ValidationProblemDetails(ModelState);
                details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                details.Status = StatusCodes.Status400BadRequest;
                return new BadRequestObjectResult(details);
            }
            catch (Exception e)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = e.Message;
                exceptionDetails.Status =
                StatusCodes.Status500InternalServerError;
                exceptionDetails.Type =
                "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                return StatusCode(
                StatusCodes.Status500InternalServerError,
                exceptionDetails);
            }

        }

    }

}
