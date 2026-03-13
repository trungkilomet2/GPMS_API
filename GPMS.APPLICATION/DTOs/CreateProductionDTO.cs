using GPMS.APPLICATION.Enum;
using GPMS.APPLICATION.Enums;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class CreateProductionDTO
    {
        public Production Data { get; set; } = null!;
        public CreateStatus Status { get; set; } = default;
        /// <summary>
        /// Value1 : Field Error
        /// Value2 : List of error messages related to that field
        /// </summary>
        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}
