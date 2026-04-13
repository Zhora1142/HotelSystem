using System.Windows;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class AddRoomTypeWindow : Window
{
    public AddRoomTypeWindow(AddRoomTypeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
