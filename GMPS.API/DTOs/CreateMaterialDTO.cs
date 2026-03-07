namespace GMPS.API.DTOs
{
    public class CreateMaterialDTO
    {
        public string MaterialName { get; set; }
        public decimal Quantity { get; set; }
        public string Uom { get; set; }
    }
}
