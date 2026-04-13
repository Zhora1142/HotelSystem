using System.Windows;
using HotelSystem.BLL.Services;

namespace HotelSystem.WPF.ViewModels;

public class AddClientViewModel : ViewModelBase
{
    private readonly IClientService _clientService;
    private string _fullName = string.Empty;
    private string _phone = string.Empty;
    private string _email = string.Empty;
    private string _login = string.Empty;

    public AddClientViewModel(IClientService clientService)
    {
        _clientService = clientService;
        SaveCommand = new RelayCommand(ExecuteSave);
    }

    public string FullName
    {
        get => _fullName;
        set { _fullName = value; OnPropertyChanged(); }
    }

    public string Phone
    {
        get => _phone;
        set { _phone = value; OnPropertyChanged(); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string Login
    {
        get => _login;
        set { _login = value; OnPropertyChanged(); }
    }

    public RelayCommand SaveCommand { get; }

    private async void ExecuteSave(object? parameter)
    {
        if (parameter is not IHavePassword passwordContainer || parameter is not Window window)
        {
            return;
        }

        try
        {
            await _clientService.CreateClientAsync(FullName, Phone, Email, Login, passwordContainer.Password);
            window.DialogResult = true;
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
