using HotelSystem.Domain;
using HotelSystem.Domain.Entities;
using HotelSystem.Domain.Security;

namespace HotelSystem.DAL;

public static class DbInitializer
{
    public static void Initialize(HotelSystemContext context)
    {
        context.Database.EnsureCreated();

        if (context.RoomTypes.Any())
        {
            return;
        }

        var clients = new[]
        {
            new Client { FullName = "Соколова Мария Андреевна", Phone = "+7 (900) 111-22-33", Email = "sokolova@example.com", Login = "sokolova", PasswordHash = PasswordHasher.HashPassword("123"), Role = UserRole.Client },
            new Client { FullName = "Орлов Игорь Петрович", Phone = "+7 (900) 222-33-44", Email = "orlov@example.com", Login = "orlov", PasswordHash = PasswordHasher.HashPassword("123"), Role = UserRole.Client },
            new Client { FullName = "ООО Деловой Партнер", Phone = "+7 (4932) 45-67-89", Email = "partner@example.com", Login = "partner", PasswordHash = PasswordHasher.HashPassword("123"), Role = UserRole.Client },
            new Client { FullName = "Гришина Елена Сергеевна", Phone = "+7 (900) 333-44-55", Email = "grishina@example.com", Login = "grishina", PasswordHash = PasswordHasher.HashPassword("123"), Role = UserRole.Client }
        };

        var roomTypes = new[]
        {
            new RoomType { Name = "Стандарт", Capacity = 2, PricePerNight = 3200m, Description = "Стандартный двухместный номер" },
            new RoomType { Name = "Комфорт", Capacity = 3, PricePerNight = 4700m, Description = "Улучшенный номер с рабочей зоной" },
            new RoomType { Name = "Люкс", Capacity = 4, PricePerNight = 7800m, Description = "Просторный номер с гостевой зоной" }
        };

        context.Clients.AddRange(clients);
        context.RoomTypes.AddRange(roomTypes);
        context.SaveChanges();

        var rooms = new[]
        {
            new Room { Number = "101", Floor = 1, RoomTypeId = roomTypes[0].Id, Status = RoomAvailabilityStatus.Available, Note = "Вид во двор" },
            new Room { Number = "102", Floor = 1, RoomTypeId = roomTypes[0].Id, Status = RoomAvailabilityStatus.Available, Note = "После ремонта" },
            new Room { Number = "201", Floor = 2, RoomTypeId = roomTypes[1].Id, Status = RoomAvailabilityStatus.Available, Note = "С рабочим столом" },
            new Room { Number = "202", Floor = 2, RoomTypeId = roomTypes[1].Id, Status = RoomAvailabilityStatus.Maintenance, Note = "Плановое обслуживание" },
            new Room { Number = "301", Floor = 3, RoomTypeId = roomTypes[2].Id, Status = RoomAvailabilityStatus.Available, Note = "Панорамный вид" },
            new Room { Number = "302", Floor = 3, RoomTypeId = roomTypes[2].Id, Status = RoomAvailabilityStatus.Available, Note = "Подходит для семьи" }
        };

        var services = new[]
        {
            new AdditionalService { Name = "Завтрак", Price = 650m, ChargeType = ServiceChargeType.PerDay, Description = "Шведский стол", IsActive = true },
            new AdditionalService { Name = "Парковка", Price = 400m, ChargeType = ServiceChargeType.PerDay, Description = "Парковочное место на территории", IsActive = true },
            new AdditionalService { Name = "Трансфер", Price = 1500m, ChargeType = ServiceChargeType.PerStay, Description = "Трансфер из аэропорта", IsActive = true },
            new AdditionalService { Name = "Прачечная", Price = 900m, ChargeType = ServiceChargeType.PerStay, Description = "Комплексная стирка вещей", IsActive = true },
            new AdditionalService { Name = "Конференц-зал", Price = 3500m, ChargeType = ServiceChargeType.PerStay, Description = "Бронирование малого зала", IsActive = true }
        };

        context.Rooms.AddRange(rooms);
        context.AdditionalServices.AddRange(services);
        context.SaveChanges();

        var today = DateTime.Today;
        var bookings = new[]
        {
            new Booking
            {
                ClientId = clients[0].Id,
                RoomId = rooms[0].Id,
                CheckInDate = today.AddDays(-1),
                CheckOutDate = today.AddDays(2),
                Status = BookingStatus.CheckedIn,
                AccommodationCost = 3200m * 3,
                ServicesCost = 0,
                TotalCost = 3200m * 3,
                Notes = "Ранний заезд согласован"
            },
            new Booking
            {
                ClientId = clients[1].Id,
                RoomId = rooms[2].Id,
                CheckInDate = today.AddDays(3),
                CheckOutDate = today.AddDays(6),
                Status = BookingStatus.Reserved,
                AccommodationCost = 4700m * 3,
                ServicesCost = 0,
                TotalCost = 4700m * 3,
                Notes = "Пожелание: высокий этаж"
            },
            new Booking
            {
                ClientId = clients[2].Id,
                RoomId = rooms[4].Id,
                CheckInDate = today.AddDays(-7),
                CheckOutDate = today.AddDays(-3),
                Status = BookingStatus.Completed,
                AccommodationCost = 7800m * 4,
                ServicesCost = 1500m,
                TotalCost = 7800m * 4 + 1500m,
                Notes = "Корпоративное размещение"
            }
        };

        rooms[0].Status = RoomAvailabilityStatus.Occupied;

        context.Bookings.AddRange(bookings);
        context.SaveChanges();

        var bookingServices = new[]
        {
            new BookingService
            {
                BookingId = bookings[2].Id,
                AdditionalServiceId = services[2].Id,
                Quantity = 1,
                ServiceNameSnapshot = services[2].Name,
                UnitPriceSnapshot = services[2].Price,
                ChargeTypeSnapshot = services[2].ChargeType,
                TotalPrice = 1500m
            }
        };

        var transactions = new[]
        {
            new BookingTransaction { BookingId = bookings[0].Id, Date = today.AddDays(-2), Description = "Создано бронирование", Amount = bookings[0].TotalCost },
            new BookingTransaction { BookingId = bookings[0].Id, Date = today.AddDays(-1), Description = "Гость заселен", Amount = 0m },
            new BookingTransaction { BookingId = bookings[1].Id, Date = today, Description = "Создано бронирование", Amount = bookings[1].TotalCost },
            new BookingTransaction { BookingId = bookings[2].Id, Date = today.AddDays(-8), Description = "Создано бронирование", Amount = bookings[2].TotalCost - 1500m },
            new BookingTransaction { BookingId = bookings[2].Id, Date = today.AddDays(-7), Description = "Добавлена услуга: Трансфер", Amount = 1500m },
            new BookingTransaction { BookingId = bookings[2].Id, Date = today.AddDays(-3), Description = "Проживание завершено", Amount = 0m }
        };

        context.BookingServices.AddRange(bookingServices);
        context.BookingTransactions.AddRange(transactions);
        context.SaveChanges();
    }
}
