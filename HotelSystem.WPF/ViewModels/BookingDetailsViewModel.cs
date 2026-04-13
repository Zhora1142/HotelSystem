using System.Collections.ObjectModel;
using HotelSystem.BLL.DTOs;
using HotelSystem.BLL.Services;

namespace HotelSystem.WPF.ViewModels;

public class BookingDetailsViewModel : ViewModelBase
{
    private readonly IBookingService _bookingService;
    private string _headerText = string.Empty;
    private ObservableCollection<BookingServiceDto> _services = new();
    private ObservableCollection<BookingTransactionDto> _transactions = new();

    public BookingDetailsViewModel(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public string HeaderText
    {
        get => _headerText;
        set { _headerText = value; OnPropertyChanged(); }
    }

    public ObservableCollection<BookingServiceDto> Services
    {
        get => _services;
        set { _services = value; OnPropertyChanged(); }
    }

    public ObservableCollection<BookingTransactionDto> Transactions
    {
        get => _transactions;
        set { _transactions = value; OnPropertyChanged(); }
    }

    public async void Initialize(int bookingId)
    {
        var details = await _bookingService.GetBookingDetailsAsync(bookingId);
        HeaderText = $"Бронирование #{details.Booking.Id}\nКлиент: {details.Booking.ClientName}\nНомер: {details.Booking.RoomNumber} ({details.Booking.RoomTypeName})\nПериод: {details.Booking.CheckInDate:dd.MM.yyyy} - {details.Booking.CheckOutDate:dd.MM.yyyy}\nИтого: {details.Booking.TotalCost:N2} руб.";
        Services = new ObservableCollection<BookingServiceDto>(details.Services);
        Transactions = new ObservableCollection<BookingTransactionDto>(details.Transactions);
    }
}
