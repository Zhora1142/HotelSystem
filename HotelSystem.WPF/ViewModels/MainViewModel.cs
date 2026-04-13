using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using HotelSystem.BLL.DTOs;
using HotelSystem.BLL.Services;
using HotelSystem.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSystem.WPF.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IClientService _clientService;
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;
    private readonly IAdditionalServiceService _additionalServiceService;
    private readonly IReportService _reportService;

    private ObservableCollection<ClientDto> _clients = new();
    private ObservableCollection<RoomTypeDto> _roomTypes = new();
    private ObservableCollection<RoomDto> _rooms = new();
    private ObservableCollection<BookingDto> _bookings = new();
    private ObservableCollection<AdditionalServiceDto> _services = new();
    private ClientDto? _selectedClient;
    private RoomTypeDto? _selectedRoomType;
    private RoomDto? _selectedRoom;
    private BookingDto? _selectedBooking;
    private AdditionalServiceDto? _selectedService;
    private string _statsText = string.Empty;
    private DateTime _reportDateFrom = DateTime.Today.AddDays(-7);
    private DateTime _reportDateTo = DateTime.Today.AddDays(7);

    public MainViewModel(
        IServiceProvider serviceProvider,
        IClientService clientService,
        IRoomService roomService,
        IBookingService bookingService,
        IAdditionalServiceService additionalServiceService,
        IReportService reportService)
    {
        _serviceProvider = serviceProvider;
        _clientService = clientService;
        _roomService = roomService;
        _bookingService = bookingService;
        _additionalServiceService = additionalServiceService;
        _reportService = reportService;

        RefreshCommand = new RelayCommand(_ => LoadData());
        AddClientCommand = new RelayCommand(_ => ExecuteAddClient());
        DeleteClientCommand = new RelayCommand(_ => ExecuteDeleteClient());
        AddRoomTypeCommand = new RelayCommand(_ => ExecuteAddRoomType());
        EditRoomTypeCommand = new RelayCommand(_ => ExecuteEditRoomType());
        AddRoomCommand = new RelayCommand(_ => ExecuteAddRoom());
        EditRoomCommand = new RelayCommand(_ => ExecuteEditRoom(), _ => SelectedRoom != null);
        SetRoomAvailableCommand = new RelayCommand(_ => ExecuteSetRoomStatus(RoomAvailabilityStatus.Available), _ => CanSetRoomAvailable());
        SetRoomMaintenanceCommand = new RelayCommand(_ => ExecuteSetRoomStatus(RoomAvailabilityStatus.Maintenance), _ => CanSetRoomMaintenance());
        AddBookingCommand = new RelayCommand(_ => ExecuteAddBooking(null));
        CheckInBookingCommand = new RelayCommand(_ => ExecuteChangeBookingStatus(BookingStatus.CheckedIn));
        CompleteBookingCommand = new RelayCommand(_ => ExecuteChangeBookingStatus(BookingStatus.Completed));
        CancelBookingCommand = new RelayCommand(_ => ExecuteChangeBookingStatus(BookingStatus.Cancelled));
        ViewBookingDetailsCommand = new RelayCommand(_ => ExecuteViewBookingDetails());
        AddServiceToBookingCommand = new RelayCommand(_ => ExecuteAddServiceToBooking());
        AddServiceCommand = new RelayCommand(_ => ExecuteAddService());
        EditServiceCommand = new RelayCommand(_ => ExecuteEditService());
        DeleteServiceCommand = new RelayCommand(_ => ExecuteDeleteService());
        ExportOccupancyCommand = new RelayCommand(_ => ExecuteExportOccupancy());
        ExportRevenueCommand = new RelayCommand(_ => ExecuteExportRevenue());
        LogoutCommand = new RelayCommand(ExecuteLogout);

        LoadData();
    }

    public ObservableCollection<ClientDto> Clients
    {
        get => _clients;
        set { _clients = value; OnPropertyChanged(); }
    }

    public ObservableCollection<RoomTypeDto> RoomTypes
    {
        get => _roomTypes;
        set { _roomTypes = value; OnPropertyChanged(); }
    }

    public ObservableCollection<RoomDto> Rooms
    {
        get => _rooms;
        set { _rooms = value; OnPropertyChanged(); }
    }

    public ObservableCollection<BookingDto> Bookings
    {
        get => _bookings;
        set { _bookings = value; OnPropertyChanged(); }
    }

    public ObservableCollection<AdditionalServiceDto> Services
    {
        get => _services;
        set { _services = value; OnPropertyChanged(); }
    }

    public ClientDto? SelectedClient
    {
        get => _selectedClient;
        set { _selectedClient = value; OnPropertyChanged(); }
    }

    public RoomDto? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            _selectedRoom = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public RoomTypeDto? SelectedRoomType
    {
        get => _selectedRoomType;
        set
        {
            _selectedRoomType = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public BookingDto? SelectedBooking
    {
        get => _selectedBooking;
        set
        {
            _selectedBooking = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public AdditionalServiceDto? SelectedService
    {
        get => _selectedService;
        set
        {
            _selectedService = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string StatsText
    {
        get => _statsText;
        set { _statsText = value; OnPropertyChanged(); }
    }

    public DateTime ReportDateFrom
    {
        get => _reportDateFrom;
        set { _reportDateFrom = value; OnPropertyChanged(); }
    }

    public DateTime ReportDateTo
    {
        get => _reportDateTo;
        set { _reportDateTo = value; OnPropertyChanged(); }
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand AddClientCommand { get; }
    public RelayCommand DeleteClientCommand { get; }
    public RelayCommand AddRoomTypeCommand { get; }
    public RelayCommand EditRoomTypeCommand { get; }
    public RelayCommand AddRoomCommand { get; }
    public RelayCommand EditRoomCommand { get; }
    public RelayCommand SetRoomAvailableCommand { get; }
    public RelayCommand SetRoomMaintenanceCommand { get; }
    public RelayCommand AddBookingCommand { get; }
    public RelayCommand CheckInBookingCommand { get; }
    public RelayCommand CompleteBookingCommand { get; }
    public RelayCommand CancelBookingCommand { get; }
    public RelayCommand ViewBookingDetailsCommand { get; }
    public RelayCommand AddServiceToBookingCommand { get; }
    public RelayCommand AddServiceCommand { get; }
    public RelayCommand EditServiceCommand { get; }
    public RelayCommand DeleteServiceCommand { get; }
    public RelayCommand ExportOccupancyCommand { get; }
    public RelayCommand ExportRevenueCommand { get; }
    public RelayCommand LogoutCommand { get; }

    public async void LoadData()
    {
        Clients = new ObservableCollection<ClientDto>((await _clientService.GetAllClientsAsync()).ToList());
        RoomTypes = new ObservableCollection<RoomTypeDto>((await _roomService.GetAllRoomTypesAsync()).ToList());
        Rooms = new ObservableCollection<RoomDto>((await _roomService.GetAllRoomsAsync()).ToList());
        Bookings = new ObservableCollection<BookingDto>((await _bookingService.GetAllBookingsAsync()).ToList());
        Services = new ObservableCollection<AdditionalServiceDto>((await _additionalServiceService.GetAllServicesAsync()).ToList());

        SelectedClient = null;
        SelectedRoomType = null;
        SelectedRoom = null;
        SelectedBooking = null;
        SelectedService = null;

        var stats = await _bookingService.GetDashboardStatsAsync();
        StatsText = $"Клиентов: {stats.TotalClients}\nНомеров: {stats.TotalRooms}\nАктивных бронирований: {stats.ActiveBookings}\nСвободных номеров: {stats.AvailableRooms}\nЗаполненность на сегодня: {stats.OccupancyTodayPercent:N2}%";
        CommandManager.InvalidateRequerySuggested();
    }

    private void ExecuteAddClient()
    {
        var window = _serviceProvider.GetRequiredService<AddClientWindow>();
        if (window.ShowDialog() == true)
        {
            LoadData();
        }
    }

    private async void ExecuteDeleteClient()
    {
        if (SelectedClient == null)
        {
            MessageBox.Show("Выберите клиента.");
            return;
        }

        if (MessageBox.Show($"Удалить клиента {SelectedClient.FullName}?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _clientService.DeleteClientAsync(SelectedClient.Id);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ExecuteAddRoomType()
    {
        var viewModel = _serviceProvider.GetRequiredService<AddRoomTypeViewModel>();
        viewModel.Initialize();
        var window = _serviceProvider.GetRequiredService<AddRoomTypeWindow>();
        window.DataContext = viewModel;
        if (window.ShowDialog() == true)
        {
            LoadData();
        }
    }

    private void ExecuteEditRoomType()
    {
        if (SelectedRoomType == null)
        {
            MessageBox.Show("Выберите тип номера.");
            return;
        }

        var viewModel = _serviceProvider.GetRequiredService<AddRoomTypeViewModel>();
        viewModel.Initialize(SelectedRoomType);
        var window = _serviceProvider.GetRequiredService<AddRoomTypeWindow>();
        window.DataContext = viewModel;

        if (window.ShowDialog() == true)
        {
            LoadData();
        }
    }

    private async void ExecuteAddRoom()
    {
        try
        {
            var viewModel = _serviceProvider.GetRequiredService<AddRoomViewModel>();
            await viewModel.InitializeAsync();
            var window = _serviceProvider.GetRequiredService<AddRoomWindow>();
            window.DataContext = viewModel;
            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка открытия окна номера: {ex.Message}");
        }
    }

    private async void ExecuteEditRoom()
    {
        if (SelectedRoom == null)
        {
            return;
        }

        try
        {
            var viewModel = _serviceProvider.GetRequiredService<AddRoomViewModel>();
            await viewModel.InitializeAsync(SelectedRoom);
            var window = _serviceProvider.GetRequiredService<AddRoomWindow>();
            window.DataContext = viewModel;
            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка открытия окна номера: {ex.Message}");
        }
    }

    private async void ExecuteSetRoomStatus(RoomAvailabilityStatus status)
    {
        try
        {
            if (SelectedRoom == null)
            {
                return;
            }

            await _roomService.UpdateRoomStatusAsync(SelectedRoom.Id, status);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ExecuteAddBooking(int? clientId)
    {
        var viewModel = _serviceProvider.GetRequiredService<AddBookingViewModel>();
        viewModel.Initialize(clientId);

        var window = _serviceProvider.GetRequiredService<AddBookingWindow>();
        window.DataContext = viewModel;
        if (window.ShowDialog() == true)
        {
            LoadData();
        }
    }

    private async void ExecuteChangeBookingStatus(BookingStatus status)
    {
        if (SelectedBooking == null)
        {
            MessageBox.Show("Выберите бронирование.");
            return;
        }

        try
        {
            await _bookingService.ChangeBookingStatusAsync(SelectedBooking.Id, status);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ExecuteViewBookingDetails()
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

    private void ExecuteAddServiceToBooking()
    {
        if (SelectedBooking == null)
        {
            MessageBox.Show("Выберите бронирование.");
            return;
        }

        var viewModel = _serviceProvider.GetRequiredService<AddServiceToBookingViewModel>();
        viewModel.Initialize(SelectedBooking.Id);

        var window = _serviceProvider.GetRequiredService<AddServiceToBookingWindow>();
        window.DataContext = viewModel;
        if (window.ShowDialog() == true)
        {
            LoadData();
        }
    }

    private void ExecuteAddService()
    {
        var viewModel = _serviceProvider.GetRequiredService<AddEditServiceViewModel>();
        viewModel.Initialize();
        var window = _serviceProvider.GetRequiredService<AddEditServiceWindow>();
        window.DataContext = viewModel;

        if (window.ShowDialog() == true)
        {
            LoadData();
        }
    }

    private void ExecuteEditService()
    {
        if (SelectedService == null)
        {
            MessageBox.Show("Выберите услугу.");
            return;
        }

        var viewModel = _serviceProvider.GetRequiredService<AddEditServiceViewModel>();
        viewModel.Initialize(SelectedService);
        var window = _serviceProvider.GetRequiredService<AddEditServiceWindow>();
        window.DataContext = viewModel;

        if (window.ShowDialog() == true)
        {
            LoadData();
        }
    }

    private async void ExecuteDeleteService()
    {
        if (SelectedService == null)
        {
            MessageBox.Show("Выберите услугу.");
            return;
        }

        if (MessageBox.Show($"Скрыть услугу {SelectedService.Name} для новых начислений?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _additionalServiceService.DeleteServiceAsync(SelectedService.Id);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private async void ExecuteExportOccupancy()
    {
        try
        {
            var rows = (await _bookingService.GetOccupancyReportAsync(ReportDateFrom, ReportDateTo)).ToList();
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Occupancy_{DateTime.Now:yyyyMMdd}",
                Filter = "HTML Document (*.html)|*.html|CSV Document (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                var content = dialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                    ? _reportService.GenerateOccupancyCsv(rows)
                    : _reportService.GenerateOccupancyHtml(ReportDateFrom, ReportDateTo, rows);

                System.IO.File.WriteAllText(dialog.FileName, content, System.Text.Encoding.UTF8);
                MessageBox.Show("Отчет сохранен.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}");
        }
    }

    private async void ExecuteExportRevenue()
    {
        try
        {
            var rows = (await _bookingService.GetRevenueReportAsync(ReportDateFrom, ReportDateTo)).ToList();
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Revenue_{DateTime.Now:yyyyMMdd}",
                Filter = "HTML Document (*.html)|*.html|CSV Document (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                var content = dialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                    ? _reportService.GenerateRevenueCsv(rows)
                    : _reportService.GenerateRevenueHtml(ReportDateFrom, ReportDateTo, rows);

                System.IO.File.WriteAllText(dialog.FileName, content, System.Text.Encoding.UTF8);
                MessageBox.Show("Отчет сохранен.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}");
        }
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

    private bool CanSetRoomAvailable()
    {
        return SelectedRoom?.CanSetAvailable == true;
    }

    private bool CanSetRoomMaintenance()
    {
        return SelectedRoom?.CanSetMaintenance == true;
    }
}
