using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace GMPS.API.DTOs
{
    public class RequestDTO<T> 
    {

        [DefaultValue(0)]
        public int PageIndex { get; set; } = 0;

        [DefaultValue(10)]
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        [DefaultValue("Name")]
        public string? SortColumn { get; set; } = "Name";

        // [SortOrderValidator] ---Config validation for SortOrder, only allow "ASC" or "DESC"
        [DefaultValue("ASC")]
        public string? SortOrder { get; set; } = "ASC";

        [DefaultValue(null)]
        public string? FilterQuery { get; set; } = null;

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    throw new NotImplementedException(validationContext?.ToString());
        //}
    }
}
