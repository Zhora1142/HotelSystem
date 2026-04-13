using System.Globalization;
using System.Windows;
using HotelSystem.BLL.DTOs;
using HotelSystem.BLL.Services;

namespace HotelSystem.WPF.ViewModels;

public class AddRoomTypeViewModel : ViewModelBase
{
    private readonly IRoomService _roomService;
    private int? _roomTypeId;
    private string _name = string.Empty;
    private string _capacity = "2";
    private string _pricePerNight = "3200";
    private string _description = string.Empty;
    private string _operationTitle = "Добавление типа номера";

    public AddRoomTypeViewModel(IRoomService roomService)
    {
        _roomService = roomService;
        SaveCommand = new RelayCommand(ExecuteSave);
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string Capacity
    {
        get => _capacity;
        set { _capacity = value; OnPropertyChanged(); }
    }

    public string PricePerNight
    {
        get => _pricePerNight;
        set { _pricePerNight = value; OnPropertyChanged(); }
    }

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public string OperationTitle
    {
        get => _operationTitle;
        set { _operationTitle = value; OnPropertyChanged(); }
    }

    public RelayCommand SaveCommand { get; }

    public void Initialize(RoomTypeDto? roomType = null)
    {
        if (roomType == null)
        {
            _roomTypeId = null;
            Name = string.Empty;
            Capacity = "2";
            PricePerNight = "3200";
            Description = string.Empty;
            OperationTitle = "Добавление типа номера";
            return;
        }

        _roomTypeId = roomType.Id;
        Name = roomType.Name;
        Capacity = roomType.Capacity.ToString(CultureInfo.InvariantCulture);
        PricePerNight = roomType.PricePerNight.ToString(CultureInfo.InvariantCulture);
        Description = roomType.Description;
        OperationTitle = "Редактирование типа номера";
    }

    private async void ExecuteSave(object? parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }

        try
        {
            var culture = CultureInfo.InvariantCulture;
            var parsedCapacity = int.Parse(Capacity);
            var parsedPrice = decimal.Parse(PricePerNight.Replace(',', '.'), culture);

            if (_roomTypeId.HasValue)
            {
                await _roomService.UpdateRoomTypeAsync(_roomTypeId.Value, Name, parsedCapacity, parsedPrice, Description);
            }
            else
            {
                await _roomService.CreateRoomTypeAsync(Name, parsedCapacity, parsedPrice, Description);
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
