using System.Windows;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class LoginWindow : Window, IHavePassword
{
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public string Password => PasswordBox.Password;
}
