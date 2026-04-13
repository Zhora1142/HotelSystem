using HotelSystem.BLL.DTOs;
using HotelSystem.DAL;
using HotelSystem.Domain;
using HotelSystem.Domain.Entities;

namespace HotelSystem.BLL.Services;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
    Task<IEnumerable<BookingDto>> GetClientBookingsAsync(int clientId);
    Task<BookingDetailsDto> GetBookingDetailsAsync(int bookingId);
    Task CreateBookingAsync(int clientId, int roomId, DateTime checkInDate, DateTime checkOutDate, string notes);
    Task ChangeBookingStatusAsync(int bookingId, BookingStatus status);
    Task AddServiceToBookingAsync(int bookingId, int additionalServiceId, int quantity);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<IEnumerable<OccupancyReportRowDto>> GetOccupancyReportAsync(DateTime dateFrom, DateTime dateTo);
    Task<IEnumerable<RevenueReportRowDto>> GetRevenueReportAsync(DateTime dateFrom, DateTime dateTo);
}

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _unitOfWork.Bookings.GetAllWithIncludeAsync(b => b.Client, b => b.Room);
        return await MapBookingsAsync(bookings);
    }

    public async Task<IEnumerable<BookingDto>> GetClientBookingsAsync(int clientId)
    {
        var bookings = await _unitOfWork.Bookings.FindAsync(b => b.ClientId == clientId);
        var clients = await _unitOfWork.Clients.GetAllAsync();
        var rooms = await _unitOfWork.Rooms.GetAllWithIncludeAsync(r => r.RoomType);

        return bookings
            .OrderByDescending(b => b.CheckInDate)
            .Select(b => MapBooking(b, clients.FirstOrDefault(c => c.Id == b.ClientId), rooms.FirstOrDefault(r => r.Id == b.RoomId)))
            .ToList();
    }

    public async Task<BookingDetailsDto> GetBookingDetailsAsync(int bookingId)
    {
        var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, b => b.Client, b => b.Room);
        if (booking == null)
        {
            throw new InvalidOperationException("Бронирование не найдено.");
        }

        var roomType = booking.Room == null ? null : await _unitOfWork.RoomTypes.GetByIdAsync(booking.Room.RoomTypeId);
        if (booking.Room != null)
        {
            booking.Room.RoomType = roomType;
        }

        var bookingServices = await _unitOfWork.BookingServices.FindAsync(bs => bs.BookingId == bookingId);
        var transactions = await _unitOfWork.BookingTransactions.FindAsync(t => t.BookingId == bookingId);

        return new BookingDetailsDto
        {
            Booking = MapBooking(booking, booking.Client, booking.Room),
            Services = bookingServices
                .OrderBy(bs => bs.Id)
                .Select(bs => new BookingServiceDto
                {
                    Id = bs.Id,
                    ServiceName = string.IsNullOrWhiteSpace(bs.ServiceNameSnapshot) ? "Неизвестная услуга" : bs.ServiceNameSnapshot,
                    Quantity = bs.Quantity,
                    TotalPrice = bs.TotalPrice
                })
                .ToList(),
            Transactions = transactions
                .OrderByDescending(t => t.Date)
                .Select(t => new BookingTransactionDto
                {
                    Date = t.Date,
                    Description = t.Description,
                    Amount = t.Amount
                })
                .ToList()
        };
    }

    public async Task CreateBookingAsync(int clientId, int roomId, DateTime checkInDate, DateTime checkOutDate, string notes)
    {
        ValidateDates(checkInDate, checkOutDate);

        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new InvalidOperationException("Клиент не найден.");
        }

        var room = await _unitOfWork.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, r => r.RoomType);
        if (room == null || room.RoomType == null)
        {
            throw new InvalidOperationException("Номер не найден.");
        }

        if (room.Status == RoomAvailabilityStatus.Maintenance)
        {
            throw new InvalidOperationException("Номер находится на обслуживании.");
        }

        await EnsureRoomIsFreeAsync(roomId, checkInDate, checkOutDate, null);

        var nights = (checkOutDate.Date - checkInDate.Date).Days;
        var accommodationCost = nights * room.RoomType.PricePerNight;

        var booking = new Booking
        {
            ClientId = clientId,
            RoomId = roomId,
            CheckInDate = checkInDate.Date,
            CheckOutDate = checkOutDate.Date,
            Status = BookingStatus.Reserved,
            AccommodationCost = accommodationCost,
            ServicesCost = 0m,
            TotalCost = accommodationCost,
            Notes = notes.Trim()
        };

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.CompleteAsync();

        await _unitOfWork.BookingTransactions.AddAsync(new BookingTransaction
        {
            BookingId = booking.Id,
            Date = DateTime.Now,
            Description = "Создано бронирование",
            Amount = accommodationCost
        });

        await _unitOfWork.CompleteAsync();
    }

    public async Task ChangeBookingStatusAsync(int bookingId, BookingStatus status)
    {
        var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, b => b.Room);
        if (booking == null || booking.Room == null)
        {
            throw new InvalidOperationException("Бронирование не найдено.");
        }

        if (status == booking.Status)
        {
            return;
        }

        switch (status)
        {
            case BookingStatus.CheckedIn:
                if (booking.Room.Status == RoomAvailabilityStatus.Maintenance)
                {
                    throw new InvalidOperationException("Номер находится на обслуживании.");
                }
                await EnsureRoomIsFreeAsync(booking.RoomId, booking.CheckInDate, booking.CheckOutDate, booking.Id);
                booking.Room.Status = RoomAvailabilityStatus.Occupied;
                break;
            case BookingStatus.Completed:
            case BookingStatus.Cancelled:
                if (booking.Room.Status != RoomAvailabilityStatus.Maintenance)
                {
                    booking.Room.Status = RoomAvailabilityStatus.Available;
                }
                break;
        }

        booking.Status = status;
        await _unitOfWork.CompleteAsync();

        await _unitOfWork.BookingTransactions.AddAsync(new BookingTransaction
        {
            BookingId = booking.Id,
            Date = DateTime.Now,
            Description = status switch
            {
                BookingStatus.CheckedIn => "Гость заселен",
                BookingStatus.Completed => "Проживание завершено",
                BookingStatus.Cancelled => "Бронирование отменено",
                BookingStatus.Reserved => "Бронирование переведено в статус бронь",
                _ => "Изменен статус бронирования"
            },
            Amount = 0m
        });

        await _unitOfWork.CompleteAsync();
    }

    public async Task AddServiceToBookingAsync(int bookingId, int additionalServiceId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Количество должно быть положительным.");
        }

        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null)
        {
            throw new InvalidOperationException("Бронирование не найдено.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Нельзя добавлять услуги к отмененному бронированию.");
        }

        var additionalService = await _unitOfWork.AdditionalServices.GetByIdAsync(additionalServiceId);
        if (additionalService == null)
        {
            throw new InvalidOperationException("Услуга не найдена.");
        }

        if (!additionalService.IsActive)
        {
            throw new InvalidOperationException("Услуга недоступна для новых начислений.");
        }

        var nights = (booking.CheckOutDate.Date - booking.CheckInDate.Date).Days;
        if (nights <= 0)
        {
            throw new InvalidOperationException("Некорректные даты проживания.");
        }

        var totalPrice = additionalService.ChargeType == ServiceChargeType.PerDay
            ? additionalService.Price * quantity * nights
            : additionalService.Price * quantity;

        booking.ServicesCost += totalPrice;
        booking.TotalCost = booking.AccommodationCost + booking.ServicesCost;

        await _unitOfWork.BookingServices.AddAsync(new HotelSystem.Domain.Entities.BookingService
        {
            BookingId = bookingId,
            AdditionalServiceId = additionalServiceId,
            Quantity = quantity,
            ServiceNameSnapshot = additionalService.Name,
            UnitPriceSnapshot = additionalService.Price,
            ChargeTypeSnapshot = additionalService.ChargeType,
            TotalPrice = totalPrice
        });

        await _unitOfWork.BookingTransactions.AddAsync(new BookingTransaction
        {
            BookingId = bookingId,
            Date = DateTime.Now,
            Description = $"Добавлена услуга: {additionalService.Name}",
            Amount = totalPrice
        });

        await _unitOfWork.CompleteAsync();
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var clients = await _unitOfWork.Clients.FindAsync(c => c.Role == UserRole.Client);
        var rooms = await _unitOfWork.Rooms.GetAllAsync();
        var bookings = await _unitOfWork.Bookings.GetAllAsync();
        var totalRooms = rooms.Count();
        var activeRoomIds = bookings
            .Where(b => b.Status == BookingStatus.Reserved || b.Status == BookingStatus.CheckedIn)
            .Select(b => b.RoomId)
            .Distinct()
            .ToHashSet();
        var occupiedToday = bookings
            .Where(b => b.Status != BookingStatus.Cancelled && b.CheckInDate.Date <= DateTime.Today && b.CheckOutDate.Date > DateTime.Today)
            .Select(b => b.RoomId)
            .Distinct()
            .Count();

        return new DashboardStatsDto
        {
            TotalClients = clients.Count(),
            TotalRooms = totalRooms,
            ActiveBookings = bookings.Count(b => b.Status == BookingStatus.Reserved || b.Status == BookingStatus.CheckedIn),
            AvailableRooms = rooms.Count(r => r.Status == RoomAvailabilityStatus.Available && !activeRoomIds.Contains(r.Id)),
            OccupancyTodayPercent = totalRooms == 0 ? 0m : Math.Round(occupiedToday * 100m / totalRooms, 2)
        };
    }

    public async Task<IEnumerable<OccupancyReportRowDto>> GetOccupancyReportAsync(DateTime dateFrom, DateTime dateTo)
    {
        if (dateTo.Date < dateFrom.Date)
        {
            throw new InvalidOperationException("Дата окончания периода должна быть не раньше даты начала.");
        }

        var rooms = await _unitOfWork.Rooms.GetAllAsync();
        var bookings = await _unitOfWork.Bookings.GetAllAsync();
        var totalRooms = rooms.Count();
        var rows = new List<OccupancyReportRowDto>();

        for (var day = dateFrom.Date; day <= dateTo.Date; day = day.AddDays(1))
        {
            var occupiedRooms = bookings
                .Where(b => b.Status != BookingStatus.Cancelled && b.CheckInDate.Date <= day && b.CheckOutDate.Date > day)
                .Select(b => b.RoomId)
                .Distinct()
                .Count();

            rows.Add(new OccupancyReportRowDto
            {
                Date = day,
                OccupiedRooms = occupiedRooms,
                TotalRooms = totalRooms,
                OccupancyPercent = totalRooms == 0 ? 0m : Math.Round(occupiedRooms * 100m / totalRooms, 2)
            });
        }

        return rows;
    }

    public async Task<IEnumerable<RevenueReportRowDto>> GetRevenueReportAsync(DateTime dateFrom, DateTime dateTo)
    {
        if (dateTo.Date < dateFrom.Date)
        {
            throw new InvalidOperationException("Дата окончания периода должна быть не раньше даты начала.");
        }

        var transactions = await _unitOfWork.BookingTransactions.FindAsync(t => t.Date.Date >= dateFrom.Date && t.Date.Date <= dateTo.Date);
        var rows = new List<RevenueReportRowDto>();

        for (var day = dateFrom.Date; day <= dateTo.Date; day = day.AddDays(1))
        {
            var dayTransactions = transactions.Where(t => t.Date.Date == day).ToList();
            var servicesRevenue = dayTransactions
                .Where(t => t.Description.StartsWith("Добавлена услуга:", StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Amount);
            var totalRevenue = dayTransactions.Where(t => t.Amount > 0).Sum(t => t.Amount);

            rows.Add(new RevenueReportRowDto
            {
                Date = day,
                AccommodationRevenue = totalRevenue - servicesRevenue,
                ServicesRevenue = servicesRevenue,
                TotalRevenue = totalRevenue
            });
        }

        return rows;
    }

    private async Task<IEnumerable<BookingDto>> MapBookingsAsync(IEnumerable<Booking> bookings)
    {
        var roomTypes = await _unitOfWork.RoomTypes.GetAllAsync();
        var rooms = bookings.Select(b => b.Room).Where(r => r != null).Cast<Room>().ToList();
        foreach (var room in rooms)
        {
            room.RoomType = roomTypes.FirstOrDefault(rt => rt.Id == room.RoomTypeId);
        }

        return bookings
            .OrderByDescending(b => b.CheckInDate)
            .Select(b => MapBooking(b, b.Client, b.Room))
            .ToList();
    }

    private static BookingDto MapBooking(Booking booking, Client? client, Room? room)
    {
        return new BookingDto
        {
            Id = booking.Id,
            ClientId = booking.ClientId,
            ClientName = client?.FullName ?? "Неизвестный клиент",
            RoomId = booking.RoomId,
            RoomNumber = room?.Number ?? "Неизвестный номер",
            RoomTypeName = room?.RoomType?.Name ?? "Неизвестный тип",
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            StatusName = booking.Status switch
            {
                BookingStatus.Reserved => "Бронь",
                BookingStatus.CheckedIn => "Заселен",
                BookingStatus.Completed => "Завершен",
                BookingStatus.Cancelled => "Отменен",
                _ => booking.Status.ToString()
            },
            AccommodationCost = booking.AccommodationCost,
            ServicesCost = booking.ServicesCost,
            TotalCost = booking.TotalCost,
            Notes = booking.Notes,
            NightsCount = (booking.CheckOutDate.Date - booking.CheckInDate.Date).Days
        };
    }

    private async Task EnsureRoomIsFreeAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, int? bookingToIgnoreId)
    {
        var bookings = await _unitOfWork.Bookings.FindAsync(b =>
            b.RoomId == roomId &&
            (b.Status == BookingStatus.Reserved || b.Status == BookingStatus.CheckedIn));

        var hasOverlap = bookings.Any(b =>
            b.Id != bookingToIgnoreId &&
            DatesOverlap(checkInDate.Date, checkOutDate.Date, b.CheckInDate.Date, b.CheckOutDate.Date));

        if (hasOverlap)
        {
            throw new InvalidOperationException("Номер уже забронирован на выбранный период.");
        }
    }

    private static void ValidateDates(DateTime checkInDate, DateTime checkOutDate)
    {
        if (checkOutDate.Date <= checkInDate.Date)
        {
            throw new InvalidOperationException("Дата выезда должна быть позже даты заезда.");
        }
    }

    private static bool DatesOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        return start1 < end2 && start2 < end1;
    }
}
