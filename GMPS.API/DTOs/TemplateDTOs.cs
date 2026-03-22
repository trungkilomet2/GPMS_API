using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class TemplateStepInputDTO
    {
        [Required]
        [StringLength(100)]
        public string PartName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int StepOrder { get; set; }
    }

    public class CreateProductionTemplateDTO
    {
        [Required]
        [StringLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<TemplateStepInputDTO> Steps { get; set; } = new();
    }

    public class TemplateViewDTO
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public List<TemplateStepViewDTO> Steps { get; set; } = new();
    }

    public class TemplateStepViewDTO
    {
        public int StepId { get; set; }
        public int StepOrder { get; set; }
        public string PartName { get; set; } = string.Empty;
    }
}