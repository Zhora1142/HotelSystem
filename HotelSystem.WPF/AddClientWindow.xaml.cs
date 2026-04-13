using System.Windows;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class AddClientWindow : Window, IHavePassword
{
    public AddClientWindow(AddClientViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public string Password => PasswordBox.Password;
}
