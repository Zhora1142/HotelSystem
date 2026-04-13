using System.Collections.ObjectModel;
using System.Windows;
using HotelSystem.BLL.DTOs;
using HotelSystem.BLL.Services;

namespace HotelSystem.WPF.ViewModels;

public class AddServiceToBookingViewModel : ViewModelBase
{
    private readonly IAdditionalServiceService _additionalServiceService;
    private readonly IBookingService _bookingService;
    private int _bookingId;
    private ObservableCollection<AdditionalServiceDto> _services = new();
    private AdditionalServiceDto? _selectedService;
    private string _quantity = "1";

    public AddServiceToBookingViewModel(IAdditionalServiceService additionalServiceService, IBookingService bookingService)
    {
        _additionalServiceService = additionalServiceService;
        _bookingService = bookingService;
        SaveCommand = new RelayCommand(ExecuteSave);
    }

    public ObservableCollection<AdditionalServiceDto> Services
    {
        get => _services;
        set { _services = value; OnPropertyChanged(); }
    }

    public AdditionalServiceDto? SelectedService
    {
        get => _selectedService;
        set { _selectedService = value; OnPropertyChanged(); }
    }

    public string Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(); }
    }

    public RelayCommand SaveCommand { get; }

    public async void Initialize(int bookingId)
    {
        _bookingId = bookingId;
        Services = new ObservableCollection<AdditionalServiceDto>((await _additionalServiceService.GetAllServicesAsync(activeOnly: true)).ToList());
        SelectedService = Services.FirstOrDefault();
    }

    private async void ExecuteSave(object? parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }

        try
        {
            if (SelectedService == null)
            {
                MessageBox.Show("Выберите услугу.");
                return;
            }

            await _bookingService.AddServiceToBookingAsync(_bookingId, SelectedService.Id, int.Parse(Quantity));
            window.DialogResult = true;
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
