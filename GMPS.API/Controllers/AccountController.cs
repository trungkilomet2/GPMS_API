using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepositories _loginRepo;
        private readonly IConfiguration _configuration;
        public AccountController(IAccountRepositories loginRepo, IConfiguration configuration)
        {
            _loginRepo = loginRepo ?? throw new ArgumentNullException(nameof(loginRepo));
            _configuration = configuration;
        }
        [HttpPost("login")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult> Login([FromBody] LoginDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {   

                    var user = await _loginRepo.Login(input.UserName!, input.Password!);

                    if (user is null) return NotFound("Invalid Login attempt.");
                    else
                    {
                        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"])), SecurityAlgorithms.HmacSha256);
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name, user.User.UserName));
                        foreach (var role in user.UserRole.Select(r => r.Name))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        // Tao Jwt Token
                        var jwtObject = new JwtSecurityToken(
                            issuer: _configuration["JWT:Issuer"],
                            audience: _configuration["JWT:Audience"],
                            claims: claims,
                            expires: DateTime.Now.AddSeconds(300),
                            signingCredentials: signingCredentials);
                        // Ky token cuoi cung va gui tra ve cho user
                        var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtObject);
                        return StatusCode(StatusCodes.Status200OK, jwtString);
                    }
                }
                else
                {
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
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
                    var newUser = new User();
                    newUser.UserName = input.FullName;
                    newUser.PasswordHash = input.Password;
                    newUser.PhoneNumber = input.PhoneNumber;
                    //Gia tri thu 2 truyen vao CreateAsync la Password goc chu khong phai HashPassword
                    //      var result = await _userManager.CreateAsync(newUser, input.Password);


                    //  if (result.Succeeded)
                    //  {
                    ////      _logger.LogInformation("User {UserName} ({email}) has been created.", newUser.UserName, newUser.Email);
                    //      return StatusCode(201, $"User '{newUser.UserName}' has been created");
                    //  }
                    //  else
                    //  {
                    //      throw new Exception(string.Format("Error: {0}", string.Join(" ", result.Errors.Select(e => e.Description))));
                    //  }

                    return Ok();
                }
                else
                {
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
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
