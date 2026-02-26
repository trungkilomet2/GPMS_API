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

        private readonly UserManager<User> _userManager;

        public AccountController(IAccountRepositories loginRepo, IConfiguration configuration,UserManager<User> userManager)
        {
            _loginRepo = loginRepo ?? throw new ArgumentNullException(nameof(loginRepo));
            _configuration = configuration;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpPost("login")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult> Login([FromBody] LoginDTO input)
        {

            var user = await _loginRepo.Login(input.UserName!, input.Password!);

            if (user is null) return NotFound("User Name or Password is wrong!");
            else
            {
                var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"])), SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();
                // claims.Add(new Claim(ClaimTypes.Name, user.Username));
                 //claims.AddRange((await _userManager.GetRolesAsync(user)).Select(r => new Claim(ClaimTypes.Role, r)));
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
    }

}
