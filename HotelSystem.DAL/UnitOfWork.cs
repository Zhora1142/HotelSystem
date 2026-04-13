using HotelSystem.DAL.Repositories;
using HotelSystem.Domain.Entities;

namespace HotelSystem.DAL;

public interface IUnitOfWork : IDisposable
{
    IRepository<Client> Clients { get; }
    IRepository<RoomType> RoomTypes { get; }
    IRepository<Room> Rooms { get; }
    IRepository<Booking> Bookings { get; }
    IRepository<AdditionalService> AdditionalServices { get; }
    IRepository<BookingService> BookingServices { get; }
    IRepository<BookingTransaction> BookingTransactions { get; }
    Task<int> CompleteAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly HotelSystemContext _context;

    public IRepository<Client> Clients { get; }
    public IRepository<RoomType> RoomTypes { get; }
    public IRepository<Room> Rooms { get; }
    public IRepository<Booking> Bookings { get; }
    public IRepository<AdditionalService> AdditionalServices { get; }
    public IRepository<BookingService> BookingServices { get; }
    public IRepository<BookingTransaction> BookingTransactions { get; }

    public UnitOfWork(HotelSystemContext context)
    {
        _context = context;
        Clients = new GenericRepository<Client>(_context);
        RoomTypes = new GenericRepository<RoomType>(_context);
        Rooms = new GenericRepository<Room>(_context);
        Bookings = new GenericRepository<Booking>(_context);
        AdditionalServices = new GenericRepository<AdditionalService>(_context);
        BookingServices = new GenericRepository<BookingService>(_context);
        BookingTransactions = new GenericRepository<BookingTransaction>(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
