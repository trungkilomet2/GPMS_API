using GMPS.API.DTOs;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/Cloudinary")]
    public class ImageController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        public ImageController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("image-upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadInputImage image)
        {
            if (image == null || image.File.Length == 0)
            {
                return BadRequest("File is empty");
            }
            var result = await _cloudinaryService.UploadImageAsync(image.File);
            return Ok(result);
        }
        [HttpGet("url-images")]
        public async Task<IActionResult> GetUrlImage(string publicId)
        {
            var url = _cloudinaryService.GetImageUrl(publicId);
            return Ok(url);
        }


    }
}
