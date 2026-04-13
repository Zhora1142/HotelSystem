using System.Windows;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class ClientWindow : Window
{
    public ClientWindow(ClientViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
