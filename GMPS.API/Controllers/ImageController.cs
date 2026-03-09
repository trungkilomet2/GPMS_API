using GMPS.API.DTOs;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        public ImageController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadInputImage image)
        {
            if (image == null || image.File.Length == 0)
            {
                return BadRequest("File is empty");
            }
            var result = await _cloudinaryService.UploadImageAsync(image.File);
            return Ok();
        }




    }
}
