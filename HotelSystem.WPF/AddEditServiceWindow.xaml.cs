using System.Windows;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class AddEditServiceWindow : Window
{
    public AddEditServiceWindow(AddEditServiceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
