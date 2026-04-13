using System.Globalization;
using System.Windows;
using HotelSystem.BLL.DTOs;
using HotelSystem.BLL.Services;
using HotelSystem.Domain;

namespace HotelSystem.WPF.ViewModels;

public class AddEditServiceViewModel : ViewModelBase
{
    private readonly IAdditionalServiceService _additionalServiceService;
    private int? _serviceId;
    private string _name = string.Empty;
    private string _price = "500";
    private ServiceChargeType _selectedChargeType = ServiceChargeType.PerStay;
    private string _description = string.Empty;
    private bool _isActive = true;
    private string _operationTitle = "Добавление услуги";

    public AddEditServiceViewModel(IAdditionalServiceService additionalServiceService)
    {
        _additionalServiceService = additionalServiceService;
        SaveCommand = new RelayCommand(ExecuteSave);
        ChargeTypes = new Dictionary<ServiceChargeType, string>
        {
            { ServiceChargeType.PerStay, "За заезд" },
            { ServiceChargeType.PerDay, "За день" }
        };
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(); }
    }

    public ServiceChargeType SelectedChargeType
    {
        get => _selectedChargeType;
        set { _selectedChargeType = value; OnPropertyChanged(); }
    }

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(); }
    }

    public string OperationTitle
    {
        get => _operationTitle;
        set { _operationTitle = value; OnPropertyChanged(); }
    }

    public Dictionary<ServiceChargeType, string> ChargeTypes { get; }

    public RelayCommand SaveCommand { get; }

    public void Initialize(AdditionalServiceDto? service = null)
    {
        if (service == null)
        {
            _serviceId = null;
            Name = string.Empty;
            Price = "500";
            SelectedChargeType = ServiceChargeType.PerStay;
            Description = string.Empty;
            IsActive = true;
            OperationTitle = "Добавление услуги";
            return;
        }

        _serviceId = service.Id;
        Name = service.Name;
        Price = service.Price.ToString(CultureInfo.InvariantCulture);
        SelectedChargeType = Enum.TryParse<ServiceChargeType>(service.ChargeTypeKey, out var chargeType) ? chargeType : ServiceChargeType.PerStay;
        Description = service.Description;
        IsActive = service.IsActive;
        OperationTitle = "Редактирование услуги";
    }

    private async void ExecuteSave(object? parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }

        try
        {
            var parsedPrice = decimal.Parse(Price.Replace(',', '.'), CultureInfo.InvariantCulture);

            if (_serviceId.HasValue)
            {
                await _additionalServiceService.UpdateServiceAsync(_serviceId.Value, Name, parsedPrice, SelectedChargeType, Description, IsActive);
            }
            else
            {
                await _additionalServiceService.CreateServiceAsync(Name, parsedPrice, SelectedChargeType, Description);
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
