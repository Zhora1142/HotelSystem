using System;
using System.Collections.Generic;

namespace HotelSystem.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Reserved;
    public decimal AccommodationCost { get; set; }
    public decimal ServicesCost { get; set; }
    public decimal TotalCost { get; set; }
    public string Notes { get; set; } = string.Empty;

    public ICollection<BookingService> Services { get; set; } = new List<BookingService>();
    public ICollection<BookingTransaction> Transactions { get; set; } = new List<BookingTransaction>();
}
