using System.Windows;
using HotelSystem.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSystem.WPF.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private string _login = string.Empty;

    public LoginViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        LoginCommand = new RelayCommand(ExecuteLogin);
    }

    public string Login
    {
        get => _login;
        set
        {
            _login = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand LoginCommand { get; }

    private async void ExecuteLogin(object? parameter)
    {
        if (parameter is not IHavePassword passwordContainer || parameter is not Window currentWindow)
        {
            return;
        }

        var password = passwordContainer.Password;

        if (Login == "admin" && password == "admin")
        {
            _serviceProvider.GetRequiredService<MainWindow>().Show();
            currentWindow.Close();
            return;
        }

        try
        {
            var clientService = _serviceProvider.GetRequiredService<IClientService>();
            var client = await clientService.AuthenticateAsync(Login, password);
            if (client == null)
            {
                MessageBox.Show("Неверный логин или пароль.");
                return;
            }

            var clientViewModel = _serviceProvider.GetRequiredService<ClientViewModel>();
            clientViewModel.Initialize(client.Id);

            var clientWindow = _serviceProvider.GetRequiredService<ClientWindow>();
            clientWindow.DataContext = clientViewModel;
            clientWindow.Show();
            currentWindow.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка входа: {ex.Message}");
        }
    }
}
