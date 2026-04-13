using System.Windows;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class AddServiceToBookingWindow : Window
{
    public AddServiceToBookingWindow(AddServiceToBookingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
