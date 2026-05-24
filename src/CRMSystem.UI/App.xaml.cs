using CRMSystem.Business.Services;
using CRMSystem.Data;
using CRMSystem.Data.Repositories;
using CRMSystem.UI.ViewModels;
using CRMSystem.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace CRMSystem.UI;

/// <summary>
/// Interaction logic for App.xaml. Builds the DI container and starts the app.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context.Configuration, services);
            })
            .Build();

        await _host.StartAsync();

        // Ensure database is created and migrations applied
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            await db.Database.MigrateAsync();
        }

        // Resolve and show the main window from the DI container
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // ---------- Data layer ----------
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=crm.db";

        services.AddDbContext<CrmDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositories
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // ---------- Business layer ----------
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IReportService, ReportService>();

        // ---------- UI layer ----------
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }
}