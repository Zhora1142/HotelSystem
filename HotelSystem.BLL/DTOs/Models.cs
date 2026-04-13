namespace HotelSystem.BLL.DTOs;

public class ClientDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}

public class RoomTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class RoomDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool CanSetAvailable { get; set; }
    public bool CanSetMaintenance { get; set; }
    public string Note { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
}

public class AdditionalServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ChargeTypeKey { get; set; } = string.Empty;
    public string ChargeTypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class BookingServiceDto
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class BookingTransactionDto
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class BookingDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal AccommodationCost { get; set; }
    public decimal ServicesCost { get; set; }
    public decimal TotalCost { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int NightsCount { get; set; }
}

public class BookingDetailsDto
{
    public BookingDto Booking { get; set; } = new();
    public List<BookingServiceDto> Services { get; set; } = new();
    public List<BookingTransactionDto> Transactions { get; set; } = new();
}

public class DashboardStatsDto
{
    public int TotalClients { get; set; }
    public int TotalRooms { get; set; }
    public int ActiveBookings { get; set; }
    public int AvailableRooms { get; set; }
    public decimal OccupancyTodayPercent { get; set; }
}

public class OccupancyReportRowDto
{
    public DateTime Date { get; set; }
    public int OccupiedRooms { get; set; }
    public int TotalRooms { get; set; }
    public decimal OccupancyPercent { get; set; }
}

public class RevenueReportRowDto
{
    public DateTime Date { get; set; }
    public decimal AccommodationRevenue { get; set; }
    public decimal ServicesRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
}
