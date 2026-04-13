using System.Windows;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
