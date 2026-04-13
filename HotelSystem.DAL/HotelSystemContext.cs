using Microsoft.EntityFrameworkCore;
using HotelSystem.Domain.Entities;

namespace HotelSystem.DAL;

public class HotelSystemContext : DbContext
{
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<RoomType> RoomTypes { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<AdditionalService> AdditionalServices { get; set; } = null!;
    public DbSet<BookingService> BookingServices { get; set; } = null!;
    public DbSet<BookingTransaction> BookingTransactions { get; set; } = null!;

    public HotelSystemContext(DbContextOptions<HotelSystemContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>()
            .HasIndex(c => c.Login)
            .IsUnique();

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.Number)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Client)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Room>()
            .HasOne(r => r.RoomType)
            .WithMany(t => t.Rooms)
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BookingService>()
            .HasOne(bs => bs.Booking)
            .WithMany(b => b.Services)
            .HasForeignKey(bs => bs.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookingService>()
            .HasOne(bs => bs.AdditionalService)
            .WithMany(s => s.BookingServices)
            .HasForeignKey(bs => bs.AdditionalServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BookingTransaction>()
            .HasOne(bt => bt.Booking)
            .WithMany(b => b.Transactions)
            .HasForeignKey(bt => bt.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
