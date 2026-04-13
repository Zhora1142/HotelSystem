using System.Windows;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class AddBookingWindow : Window
{
    public AddBookingWindow(AddBookingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
