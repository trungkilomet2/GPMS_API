namespace GMPS.API.DTOs
{
    public class CreateManualOrderRequest
    {
        public CreateManualOrderDTO Order { get; set; }
        public CreateGuest Guest { get; set; }
    }
}
