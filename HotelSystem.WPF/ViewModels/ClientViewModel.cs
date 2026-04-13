using System.Collections.ObjectModel;
using System.Windows;
using HotelSystem.BLL.DTOs;
using HotelSystem.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSystem.WPF.ViewModels;

public class ClientViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IClientService _clientService;
    private readonly IBookingService _bookingService;

    private int _clientId;
    private string _headerText = "Личный кабинет";
    private ObservableCollection<BookingDto> _bookings = new();
    private BookingDto? _selectedBooking;

    public ClientViewModel(IServiceProvider serviceProvider, IClientService clientService, IBookingService bookingService)
    {
        _serviceProvider = serviceProvider;
        _clientService = clientService;
        _bookingService = bookingService;

        RefreshCommand = new RelayCommand(_ => LoadData());
        CreateBookingCommand = new RelayCommand(_ => ExecuteCreateBooking());
        ViewDetailsCommand = new RelayCommand(_ => ExecuteViewDetails());
        LogoutCommand = new RelayCommand(ExecuteLogout);
    }

    public string HeaderText
    {
        get => _headerText;
        set { _headerText = value; OnPropertyChanged(); }
    }

    public ObservableCollection<BookingDto> Bookings
    {
        get => _bookings;
        set { _bookings = value; OnPropertyChanged(); }
    }

    public BookingDto? SelectedBooking
    {
        get => _selectedBooking;
        set { _selectedBooking = value; OnPropertyChanged(); }
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand CreateBookingCommand { get; }
    public RelayCommand ViewDetailsCommand { get; }
    public RelayCommand LogoutCommand { get; }

    public void Initialize(int clientId)
    {
        _clientId = clientId;
        LoadData();
    }

    public async void LoadData()
    {
        var client = await _clientService.GetClientByIdAsync(_clientId);
        if (client != null)
        {
            HeaderText = $"{client.FullName}\n{client.Phone} | {client.Email}";
        }

        Bookings = new ObservableCollection<BookingDto>((await _bookingService.GetClientBookingsAsync(_clientId)).ToList());
    }

    private void ExecuteCreateBooking()
    {
        var viewModel = _serviceProvider.GetRequiredService<AddBookingViewModel>();
        viewModel.Initialize(_clientId);

        var window = _serviceProvider.GetRequiredService<AddBookingWindow>();
        window.DataContext = viewModel;
        if (window.ShowDialog() == true)
        {
            LoadData();
        }
    }

    private void ExecuteViewDetails()
    {
        if (SelectedBooking == null)
        {
            MessageBox.Show("Выберите бронирование.");
            return;
        }

        var viewModel = _serviceProvider.GetRequiredService<BookingDetailsViewModel>();
        viewModel.Initialize(SelectedBooking.Id);

        var window = _serviceProvider.GetRequiredService<BookingDetailsWindow>();
        window.DataContext = viewModel;
        window.ShowDialog();
    }

    private void ExecuteLogout(object? parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }

        _serviceProvider.GetRequiredService<LoginWindow>().Show();
        window.Close();
    }
}
