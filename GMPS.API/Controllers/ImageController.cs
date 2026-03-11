using GMPS.API.DTOs;
using GPMS.DOMAIN.Constants;
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
            var result = await _cloudinaryService.UploadImageAsync(image.File,CloudinaryConstrants.Cloudinary_Order_Image_Folder);
            return Ok(result);
        }
        [HttpGet("url-images")]
        public async Task<IActionResult> GetUrlImage(string publicId)
        {
            var url = _cloudinaryService.GetImageUrl(publicId);
            return Ok(url);
        }

        //Test API TEMPLATE
        // Require File Folder <= 10MB
        [HttpPost("template-file-upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadTemplateFile([FromForm] UploadInputImage fileInput)
        {
            if (fileInput == null || fileInput.File == null || fileInput.File.Length == 0)
            {
                return BadRequest("File is empty");
            }

            var result = await _cloudinaryService.UploadTemplateFileAsync(
                fileInput.File,
                CloudinaryConstrants.Cloudinary_Template_File_Folder);
            return Ok(result);
        }


    }
}
