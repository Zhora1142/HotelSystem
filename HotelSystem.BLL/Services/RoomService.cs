using HotelSystem.BLL.DTOs;
using HotelSystem.DAL;
using HotelSystem.Domain;
using HotelSystem.Domain.Entities;

namespace HotelSystem.BLL.Services;

public interface IRoomService
{
    Task<IEnumerable<RoomTypeDto>> GetAllRoomTypesAsync();
    Task<IEnumerable<RoomDto>> GetAllRoomsAsync();
    Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate);
    Task CreateRoomTypeAsync(string name, int capacity, decimal pricePerNight, string description);
    Task UpdateRoomTypeAsync(int id, string name, int capacity, decimal pricePerNight, string description);
    Task CreateRoomAsync(string number, int floor, int roomTypeId, RoomAvailabilityStatus status, string note);
    Task UpdateRoomAsync(int id, string number, int floor, string note);
    Task UpdateRoomStatusAsync(int roomId, RoomAvailabilityStatus status);
}

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoomService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<RoomTypeDto>> GetAllRoomTypesAsync()
    {
        var roomTypes = await _unitOfWork.RoomTypes.GetAllAsync();
        return roomTypes
            .OrderBy(rt => rt.Name)
            .Select(rt => new RoomTypeDto
            {
                Id = rt.Id,
                Name = rt.Name,
                Capacity = rt.Capacity,
                PricePerNight = rt.PricePerNight,
                Description = rt.Description
            })
            .ToList();
    }

    public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
    {
        var rooms = await _unitOfWork.Rooms.GetAllWithIncludeAsync(r => r.RoomType);
        var activeBookings = await _unitOfWork.Bookings.FindAsync(b =>
            b.Status == BookingStatus.Reserved || b.Status == BookingStatus.CheckedIn);

        var activeBookingsByRoomId = activeBookings
            .GroupBy(b => b.RoomId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return rooms
            .OrderBy(r => r.Number)
            .Select(r => MapRoom(r, GetDisplayStatus(r, activeBookingsByRoomId), activeBookingsByRoomId.ContainsKey(r.Id)))
            .ToList();
    }

    public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate)
    {
        ValidateDates(checkInDate, checkOutDate);

        var rooms = await _unitOfWork.Rooms.GetAllWithIncludeAsync(r => r.RoomType);
        var activeBookings = await _unitOfWork.Bookings.FindAsync(b =>
            b.Status == BookingStatus.Reserved || b.Status == BookingStatus.CheckedIn);

        var unavailableRoomIds = activeBookings
            .Where(b => DatesOverlap(checkInDate.Date, checkOutDate.Date, b.CheckInDate.Date, b.CheckOutDate.Date))
            .Select(b => b.RoomId)
            .Distinct()
            .ToHashSet();

        return rooms
            .Where(r => r.Status != RoomAvailabilityStatus.Maintenance && !unavailableRoomIds.Contains(r.Id))
            .OrderBy(r => r.Number)
            .Select(r => MapRoom(r, "Свободен", false))
            .ToList();
    }

    public async Task CreateRoomTypeAsync(string name, int capacity, decimal pricePerNight, string description)
    {
        ValidateRoomType(name, capacity, pricePerNight);

        await _unitOfWork.RoomTypes.AddAsync(new RoomType
        {
            Name = name.Trim(),
            Capacity = capacity,
            PricePerNight = pricePerNight,
            Description = description.Trim()
        });

        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateRoomTypeAsync(int id, string name, int capacity, decimal pricePerNight, string description)
    {
        ValidateRoomType(name, capacity, pricePerNight);

        var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(id);
        if (roomType == null)
        {
            throw new InvalidOperationException("Тип номера не найден.");
        }

        roomType.Name = name.Trim();
        roomType.Capacity = capacity;
        roomType.PricePerNight = pricePerNight;
        roomType.Description = description.Trim();

        await _unitOfWork.CompleteAsync();
    }

    public async Task CreateRoomAsync(string number, int floor, int roomTypeId, RoomAvailabilityStatus status, string note)
    {
        ValidateRoom(number, floor);

        var existing = await _unitOfWork.Rooms.FirstOrDefaultAsync(r => r.Number == number);
        if (existing != null)
        {
            throw new InvalidOperationException("Комната с таким номером уже существует.");
        }

        var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(roomTypeId);
        if (roomType == null)
        {
            throw new InvalidOperationException("Тип номера не найден.");
        }

        await _unitOfWork.Rooms.AddAsync(new Room
        {
            Number = number.Trim(),
            Floor = floor,
            RoomTypeId = roomTypeId,
            Status = status,
            Note = note.Trim()
        });

        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateRoomAsync(int id, string number, int floor, string note)
    {
        ValidateRoom(number, floor);

        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room == null)
        {
            throw new InvalidOperationException("Номер не найден.");
        }

        var normalizedNumber = number.Trim();
        var existing = await _unitOfWork.Rooms.FirstOrDefaultAsync(r => r.Number == normalizedNumber);
        if (existing != null && existing.Id != id)
        {
            throw new InvalidOperationException("Комната с таким номером уже существует.");
        }

        room.Number = normalizedNumber;
        room.Floor = floor;
        room.Note = note.Trim();

        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateRoomStatusAsync(int roomId, RoomAvailabilityStatus status)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
        if (room == null)
        {
            throw new InvalidOperationException("Номер не найден.");
        }

        var activeBookings = await _unitOfWork.Bookings.FindAsync(b =>
            b.RoomId == roomId && (b.Status == BookingStatus.Reserved || b.Status == BookingStatus.CheckedIn));

        if (status == RoomAvailabilityStatus.Available)
        {
            if (activeBookings.Any())
            {
                throw new InvalidOperationException("Нельзя сделать свободным номер с активным бронированием или проживанием.");
            }

            if (room.Status != RoomAvailabilityStatus.Maintenance)
            {
                throw new InvalidOperationException("Сделать номер свободным можно только после снятия с обслуживания.");
            }
        }

        if (status == RoomAvailabilityStatus.Maintenance)
        {
            if (activeBookings.Any())
            {
                throw new InvalidOperationException("Нельзя отправить номер в обслуживание при активных бронированиях.");
            }
        }

        room.Status = status;
        await _unitOfWork.CompleteAsync();
    }

    private static void ValidateRoomType(string name, int capacity, decimal pricePerNight)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Введите название типа номера.");
        }

        if (capacity <= 0)
        {
            throw new InvalidOperationException("Вместимость должна быть положительной.");
        }

        if (pricePerNight <= 0)
        {
            throw new InvalidOperationException("Цена за ночь должна быть положительной.");
        }
    }

    private static void ValidateRoom(string number, int floor)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new InvalidOperationException("Введите номер комнаты.");
        }

        if (floor <= 0)
        {
            throw new InvalidOperationException("Этаж должен быть положительным.");
        }
    }

    private static RoomDto MapRoom(Room room, string statusName, bool hasActiveBookings)
    {
        return new RoomDto
        {
            Id = room.Id,
            Number = room.Number,
            Floor = room.Floor,
            StatusName = statusName,
            CanSetAvailable = room.Status == RoomAvailabilityStatus.Maintenance && !hasActiveBookings,
            CanSetMaintenance = room.Status != RoomAvailabilityStatus.Maintenance && !hasActiveBookings,
            Note = room.Note,
            RoomTypeId = room.RoomTypeId,
            RoomTypeName = room.RoomType?.Name ?? "Не задан",
            Capacity = room.RoomType?.Capacity ?? 0,
            PricePerNight = room.RoomType?.PricePerNight ?? 0m
        };
    }

    private static string GetDisplayStatus(Room room, IReadOnlyDictionary<int, List<Booking>> activeBookingsByRoomId)
    {
        if (room.Status == RoomAvailabilityStatus.Maintenance)
        {
            return "Обслуживание";
        }

        if (activeBookingsByRoomId.TryGetValue(room.Id, out var roomBookings))
        {
            if (roomBookings.Any(b => b.Status == BookingStatus.CheckedIn))
            {
                return "Занят";
            }

            if (roomBookings.Any(b => b.Status == BookingStatus.Reserved))
            {
                return "Забронирован";
            }
        }

        return room.Status switch
        {
            RoomAvailabilityStatus.Occupied => "Занят",
            RoomAvailabilityStatus.Available => "Свободен",
            RoomAvailabilityStatus.Maintenance => "Обслуживание",
            _ => room.Status.ToString()
        };
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
