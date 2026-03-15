using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateProductionDTO
    {
        [Required(ErrorMessage = "PM không được để trống")]
        [Range(1,int.MaxValue,ErrorMessage = "Yêu cầu nhập input bằng số")]
        public int PmId { get; set; }
        [Required(ErrorMessage = "OrderId không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Yêu cầu nhập input bằng số")]
        public int OrderId { get; set; }
    }
}