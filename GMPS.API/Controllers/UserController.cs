using GPMS.APPLICATION.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        public readonly IUserInterface userInterface;






    }
}
