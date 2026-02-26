using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   // [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepositories _userRepo;

        private readonly IConfiguration _configuration;

        public UserController(IUserRepositories userInterface, IConfiguration configuration)
        {
            _userRepo = userInterface ?? throw new ArgumentNullException(nameof(userInterface));
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<RestDTO<IEnumerable<User>>> GetUser([FromQuery] RequestDTO<User> input)
        {
            var result = await _userRepo.GetAllUser();
            return new RestDTO<IEnumerable<User>>
            {
                Data = result,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = result.Count(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO (Url.Action(null,"User", new { input.PageIndex, input.PageSize }, Request.Scheme)!,"self","GET")
                }
            };
        }
    }







}
