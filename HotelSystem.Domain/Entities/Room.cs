using System.Collections.Generic;

namespace HotelSystem.Domain.Entities;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public RoomAvailabilityStatus Status { get; set; } = RoomAvailabilityStatus.Available;
    public string Note { get; set; } = string.Empty;

    public int RoomTypeId { get; set; }
    public RoomType? RoomType { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
