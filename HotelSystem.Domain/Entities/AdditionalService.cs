using System.Collections.Generic;

namespace HotelSystem.Domain.Entities;

public class AdditionalService
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ServiceChargeType ChargeType { get; set; } = ServiceChargeType.PerStay;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}
