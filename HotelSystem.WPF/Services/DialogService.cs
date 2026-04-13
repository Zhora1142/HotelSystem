using Microsoft.Extensions.DependencyInjection;

namespace HotelSystem.WPF.Services;

public interface IDialogService
{
    void Show<TWindow>() where TWindow : System.Windows.Window;
    bool? ShowDialog<TWindow>() where TWindow : System.Windows.Window;
}

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Show<TWindow>() where TWindow : System.Windows.Window
    {
        _serviceProvider.GetRequiredService<TWindow>().Show();
    }

    public bool? ShowDialog<TWindow>() where TWindow : System.Windows.Window
    {
        return _serviceProvider.GetRequiredService<TWindow>().ShowDialog();
    }
}
