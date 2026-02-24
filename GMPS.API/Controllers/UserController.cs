using GPMS.APPLICATION.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        public readonly IUserInterface _userInterface;

        public UserController(IUserInterface userInterface)
        {
            _userInterface = userInterface ?? throw new ArgumentNullException(nameof(userInterface));
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var data = await _userInterface.GetUser();
            return Ok(data);


        }
    }
}
