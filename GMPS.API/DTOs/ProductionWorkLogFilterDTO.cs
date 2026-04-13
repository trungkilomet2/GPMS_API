using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class ProductionWorkLogFilterDTO
    {
        [Range(1, int.MaxValue)]
        public int? WorkerId { get; set; }
    }
}
