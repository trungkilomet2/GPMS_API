using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class ListAssignForPM
    {
        [Required]
        [Range(1,int.MaxValue)]
        public int PMId { get; set; }
        [Required]
        public DateTime fromDate { get; set; }
        [Required]
        public DateTime toDate { get; set; }

    }
}
