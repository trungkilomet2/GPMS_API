using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {

        private readonly IUserRepositories _userRepo;

        private readonly IConfiguration _configuration;

        public LoginController(IUserRepositories userInterface, IConfiguration configuration)
        {
            _userRepo = userInterface ?? throw new ArgumentNullException(nameof(userInterface));
            _configuration = configuration;
        }


        [HttpPost]
        public async Task<ActionResult> Login(LoginDTO input)
        {

            var user = await _userRepo.Login(input.UserName!, input.Password!);

            if (user is null) return NotFound("User Name or Password is wrong!");
            else
            {
                var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"])), SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();
                // claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                // claims.AddRange((await _userManager.GetRolesAsync(user)).Select(r => new Claim(ClaimTypes.Role, r)));
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
