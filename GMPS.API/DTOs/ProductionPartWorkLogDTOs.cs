using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreatePartWorkLogDTO
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class UpdatePartWorkLogDTO
    {
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}