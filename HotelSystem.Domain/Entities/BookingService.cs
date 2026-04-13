namespace HotelSystem.Domain.Entities;

public class BookingService
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking? Booking { get; set; }

    public int AdditionalServiceId { get; set; }
    public AdditionalService? AdditionalService { get; set; }

    public int Quantity { get; set; }
    public string ServiceNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public ServiceChargeType ChargeTypeSnapshot { get; set; } = ServiceChargeType.PerStay;
    public decimal TotalPrice { get; set; }
}
