using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using HotelSystem.BLL.DTOs;
using HotelSystem.BLL.Services;
using HotelSystem.Domain;

namespace HotelSystem.WPF.ViewModels;

public class AddRoomViewModel : ViewModelBase
{
    private readonly IRoomService _roomService;
    private int? _roomId;
    private string _number = string.Empty;
    private string _floor = "1";
    private RoomTypeDto? _selectedRoomType;
    private RoomAvailabilityStatus _selectedStatus = RoomAvailabilityStatus.Available;
    private string _note = string.Empty;
    private ObservableCollection<RoomTypeDto> _roomTypes = new();
    private string _operationTitle = "Добавление номера";
    private bool _isRoomTypeSelectionEnabled = true;
    private bool _isStatusSelectionEnabled = true;

    public AddRoomViewModel(IRoomService roomService)
    {
        _roomService = roomService;
        SaveCommand = new RelayCommand(ExecuteSave);
        Statuses = new Dictionary<RoomAvailabilityStatus, string>
        {
            { RoomAvailabilityStatus.Available, "Свободен" },
            { RoomAvailabilityStatus.Maintenance, "Обслуживание" },
            { RoomAvailabilityStatus.Occupied, "Занят" }
        };
    }

    public string Number
    {
        get => _number;
        set { _number = value; OnPropertyChanged(); }
    }

    public string Floor
    {
        get => _floor;
        set { _floor = value; OnPropertyChanged(); }
    }

    public RoomTypeDto? SelectedRoomType
    {
        get => _selectedRoomType;
        set { _selectedRoomType = value; OnPropertyChanged(); }
    }

    public RoomAvailabilityStatus SelectedStatus
    {
        get => _selectedStatus;
        set { _selectedStatus = value; OnPropertyChanged(); }
    }

    public string Note
    {
        get => _note;
        set { _note = value; OnPropertyChanged(); }
    }

    public ObservableCollection<RoomTypeDto> RoomTypes
    {
        get => _roomTypes;
        set { _roomTypes = value; OnPropertyChanged(); }
    }

    public string OperationTitle
    {
        get => _operationTitle;
        set { _operationTitle = value; OnPropertyChanged(); }
    }

    public bool IsRoomTypeSelectionEnabled
    {
        get => _isRoomTypeSelectionEnabled;
        set { _isRoomTypeSelectionEnabled = value; OnPropertyChanged(); }
    }

    public bool IsStatusSelectionEnabled
    {
        get => _isStatusSelectionEnabled;
        set { _isStatusSelectionEnabled = value; OnPropertyChanged(); }
    }

    public Dictionary<RoomAvailabilityStatus, string> Statuses { get; }

    public RelayCommand SaveCommand { get; }

    public async Task InitializeAsync(RoomDto? room = null)
    {
        RoomTypes = new ObservableCollection<RoomTypeDto>((await _roomService.GetAllRoomTypesAsync()).ToList());

        if (room == null)
        {
            _roomId = null;
            Number = string.Empty;
            Floor = "1";
            Note = string.Empty;
            SelectedStatus = RoomAvailabilityStatus.Available;
            SelectedRoomType = RoomTypes.FirstOrDefault();
            OperationTitle = "Добавление номера";
            IsRoomTypeSelectionEnabled = true;
            IsStatusSelectionEnabled = true;
            return;
        }

        _roomId = room.Id;
        Number = room.Number;
        Floor = room.Floor.ToString(CultureInfo.InvariantCulture);
        Note = room.Note;
        SelectedRoomType = RoomTypes.FirstOrDefault(rt => rt.Id == room.RoomTypeId) ?? RoomTypes.FirstOrDefault();
        SelectedStatus = room.StatusName switch
        {
            "Обслуживание" => RoomAvailabilityStatus.Maintenance,
            "Занят" => RoomAvailabilityStatus.Occupied,
            _ => RoomAvailabilityStatus.Available
        };
        OperationTitle = "Редактирование номера";
        IsRoomTypeSelectionEnabled = false;
        IsStatusSelectionEnabled = false;
    }

    private async void ExecuteSave(object? parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }

        try
        {
            if (SelectedRoomType == null)
            {
                MessageBox.Show("Выберите тип номера.");
                return;
            }

            var parsedFloor = int.Parse(Floor, CultureInfo.InvariantCulture);

            if (_roomId.HasValue)
            {
                await _roomService.UpdateRoomAsync(
                    _roomId.Value,
                    Number,
                    parsedFloor,
                    Note);
            }
            else
            {
                await _roomService.CreateRoomAsync(
                    Number,
                    parsedFloor,
                    SelectedRoomType.Id,
                    SelectedStatus,
                    Note);
            }

            window.DialogResult = true;
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
