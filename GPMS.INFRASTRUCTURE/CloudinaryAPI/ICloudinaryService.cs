using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.CloudinaryAPI
{
    public interface ICloudinaryService
    {
        Task<UploadImageResponseDTO> UploadImageAsync(IFormFile file,string imageType);
        Task<UploadImageResponseDTO> UploadTemplateFileAsync(IFormFile file, string folder);
        string GetImageUrl(string publicId);


    }
}
