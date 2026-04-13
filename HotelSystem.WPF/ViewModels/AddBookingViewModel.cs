using System.Collections.ObjectModel;
using System.Windows;
using HotelSystem.BLL.DTOs;
using HotelSystem.BLL.Services;

namespace HotelSystem.WPF.ViewModels;

public class AddBookingViewModel : ViewModelBase
{
    private readonly IClientService _clientService;
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;

    private ObservableCollection<ClientDto> _clients = new();
    private ObservableCollection<RoomDto> _availableRooms = new();
    private ClientDto? _selectedClient;
    private RoomDto? _selectedRoom;
    private DateTime _checkInDate = DateTime.Today.AddDays(1);
    private DateTime _checkOutDate = DateTime.Today.AddDays(2);
    private string _notes = string.Empty;

    public AddBookingViewModel(IClientService clientService, IRoomService roomService, IBookingService bookingService)
    {
        _clientService = clientService;
        _roomService = roomService;
        _bookingService = bookingService;

        RefreshRoomsCommand = new RelayCommand(_ => LoadAvailableRooms());
        SaveCommand = new RelayCommand(ExecuteSave);
    }

    public ObservableCollection<ClientDto> Clients
    {
        get => _clients;
        set { _clients = value; OnPropertyChanged(); }
    }

    public ObservableCollection<RoomDto> AvailableRooms
    {
        get => _availableRooms;
        set { _availableRooms = value; OnPropertyChanged(); }
    }

    public ClientDto? SelectedClient
    {
        get => _selectedClient;
        set { _selectedClient = value; OnPropertyChanged(); }
    }

    public RoomDto? SelectedRoom
    {
        get => _selectedRoom;
        set { _selectedRoom = value; OnPropertyChanged(); }
    }

    public DateTime CheckInDate
    {
        get => _checkInDate;
        set { _checkInDate = value; OnPropertyChanged(); }
    }

    public DateTime CheckOutDate
    {
        get => _checkOutDate;
        set { _checkOutDate = value; OnPropertyChanged(); }
    }

    public string Notes
    {
        get => _notes;
        set { _notes = value; OnPropertyChanged(); }
    }

    public RelayCommand RefreshRoomsCommand { get; }
    public RelayCommand SaveCommand { get; }

    public async void Initialize(int? fixedClientId)
    {
        var clients = (await _clientService.GetAllClientsAsync()).ToList();
        if (fixedClientId.HasValue)
        {
            clients = clients.Where(c => c.Id == fixedClientId.Value).ToList();
        }

        Clients = new ObservableCollection<ClientDto>(clients);
        SelectedClient = Clients.FirstOrDefault();
        LoadAvailableRooms();
    }

    public async void LoadAvailableRooms()
    {
        try
        {
            AvailableRooms = new ObservableCollection<RoomDto>((await _roomService.GetAvailableRoomsAsync(CheckInDate, CheckOutDate)).ToList());
            SelectedRoom = AvailableRooms.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private async void ExecuteSave(object? parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }

        try
        {
            if (SelectedClient == null || SelectedRoom == null)
            {
                MessageBox.Show("Выберите клиента и номер.");
                return;
            }

            await _bookingService.CreateBookingAsync(SelectedClient.Id, SelectedRoom.Id, CheckInDate, CheckOutDate, Notes);
            window.DialogResult = true;
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
