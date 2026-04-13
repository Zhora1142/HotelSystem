using System.Windows;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class BookingDetailsWindow : Window
{
    public BookingDetailsWindow(BookingDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
