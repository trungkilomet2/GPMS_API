using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace GPMS.INFRASTRUCTURE.CloudinaryAPI
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            Account account = new Account(cloudName, apiKey, apiSecret);

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing. Please set Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret.");
            }

            Cloudinary cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true; // Sử dụng HTTPS cho URL trả về
        }

        public async Task<UploadImageResponseDTO> UploadImageAsync(IFormFile file)
        {

            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty.", nameof(file));
            }
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
                Folder = "order_image"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            return new UploadImageResponseDTO
            {
                Url = uploadResult.SecureUrl?.ToString() ?? string.Empty,
                PublicId = uploadResult.PublicId
            };
        }

        public string GetImageUrl(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                throw new ArgumentException("Public ID is null or empty.", nameof(publicId));
            }
            var imageUrl = _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
            return imageUrl;
        }
    }
}
