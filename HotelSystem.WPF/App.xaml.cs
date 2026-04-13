using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HotelSystem.BLL.Services;
using HotelSystem.DAL;
using HotelSystem.WPF.Services;
using HotelSystem.WPF.ViewModels;

namespace HotelSystem.WPF;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        const string connectionString = "Host=localhost;Port=5432;Database=hotel_system_db;Username=postgres;Password=postgres";

        services.AddDbContext<HotelSystemContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IAdditionalServiceService, AdditionalServiceService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddSingleton<IDialogService, DialogService>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<ClientWindow>();
        services.AddTransient<AddClientWindow>();
        services.AddTransient<AddRoomTypeWindow>();
        services.AddTransient<AddRoomWindow>();
        services.AddTransient<AddBookingWindow>();
        services.AddTransient<BookingDetailsWindow>();
        services.AddTransient<AddServiceToBookingWindow>();
        services.AddTransient<AddEditServiceWindow>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<ClientViewModel>();
        services.AddTransient<AddClientViewModel>();
        services.AddTransient<AddRoomTypeViewModel>();
        services.AddTransient<AddRoomViewModel>();
        services.AddTransient<AddBookingViewModel>();
        services.AddTransient<BookingDetailsViewModel>();
        services.AddTransient<AddServiceToBookingViewModel>();
        services.AddTransient<AddEditServiceViewModel>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HotelSystemContext>();
        DbInitializer.Initialize(context);

        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }
}
