using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateProductionDTO
    {
        [Required]   
        public int PmId { get; set; }
        [Required]
        public int OrderId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int StatusId { get; set; }
    }
}