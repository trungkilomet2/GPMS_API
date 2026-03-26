using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class RejectProductionDTO
    {
        [Required]
        [Range(1,int.MaxValue)]
        public int UserId { get; set; }

        public DateTime CreateAt { get; set; }
        [Required]
        public string Reason { get; set; } = string.Empty;
    }



}
