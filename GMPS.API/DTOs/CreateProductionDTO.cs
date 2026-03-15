using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateProductionDTO
    {
        [Required(ErrorMessage = "PM không được để trống")]
        public int PmId { get; set; }
        [Required(ErrorMessage = "OrderId không được để trống")]
        public int OrderId { get; set; }
    }
}