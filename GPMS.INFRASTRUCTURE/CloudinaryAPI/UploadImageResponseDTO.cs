using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.CloudinaryAPI
{
    public class UploadImageResponseDTO
    {
        public string Url { get; set; } = string.Empty; 
        public string PublicId { get; set; } = string.Empty;    
    }
}
