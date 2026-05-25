using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMSystem.Business.Services;
using CRMSystem.Domain.Enums;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CRMSystem.UI.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly IReportService _reportService;

    // Stats cards
    [ObservableProperty]
    private int _totalClients;

    [ObservableProperty]
    private int _totalContacts;

    [ObservableProperty]
    private int _contactsLast30Days;

    [ObservableProperty]
    private int _contactsLast7Days;

    // Chart series
    [ObservableProperty]
    private IEnumerable<ISeries> _clientStatusSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private IEnumerable<ISeries> _contactTypeSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _contactTypeXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _contactTypeYAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private string _statusMessage = "Loading...";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasNoData;

    public ReportsViewModel(IReportService reportService)
    {
        _reportService = reportService;
        _ = LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading reports...";

            // Stats
            TotalClients = await _reportService.GetTotalClientCountAsync();
            TotalContacts = await _reportService.GetTotalContactCountAsync();
            ContactsLast30Days = await _reportService.GetContactsInLastDaysAsync(30);
            ContactsLast7Days = await _reportService.GetContactsInLastDaysAsync(7);

            HasNoData = TotalClients == 0 && TotalContacts == 0;

            // Pie chart — clients by status
            var byStatus = await _reportService.GetClientCountByStatusAsync();
            ClientStatusSeries = BuildPieSeries(byStatus);

            // Bar chart — contacts by type
            var byType = await _reportService.GetContactCountByTypeAsync();
            BuildBarChart(byType);

            StatusMessage = $"Last updated: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static IEnumerable<ISeries> BuildPieSeries(Dictionary<ClientStatus, int> data)
    {
        // Build a pie series per status, but only include non-zero values for cleaner display.
        var colors = new Dictionary<ClientStatus, SKColor>
        {
            { ClientStatus.New, new SKColor(96, 165, 250) },           // blue
            { ClientStatus.Active, new SKColor(74, 222, 128) },        // green
            { ClientStatus.InNegotiation, new SKColor(251, 191, 36) }, // amber
            { ClientStatus.Closed, new SKColor(148, 163, 184) }        // gray
        };

        return Enum.GetValues<ClientStatus>()
            .Where(s => data.TryGetValue(s, out var count) && count > 0)
            .Select(status => (ISeries)new PieSeries<int>
            {
                Name = status.ToString(),
                Values = new[] { data[status] },
                Fill = new SolidColorPaint(colors[status]),
                DataLabelsSize = 14,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString()
            })
            .ToList();
    }

    private void BuildBarChart(Dictionary<ContactType, int> data)
    {
        var types = Enum.GetValues<ContactType>().ToArray();
        var values = types.Select(t => data.GetValueOrDefault(t, 0)).ToArray();
        var labels = types.Select(t => t.ToString()).ToArray();

        ContactTypeSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Contacts",
                Values = values,
                Fill = new SolidColorPaint(new SKColor(43, 49, 61)) // matches our brand
            }
        };

        ContactTypeXAxes = new[]
        {
            new Axis
            {
                Labels = labels,
                LabelsRotation = 0,
                TextSize = 12,
                LabelsPaint = new SolidColorPaint(SKColors.Black)
            }
        };

        ContactTypeYAxes = new[]
        {
            new Axis
            {
                MinLimit = 0,
                MinStep = 1,
                TextSize = 12,
                LabelsPaint = new SolidColorPaint(SKColors.Black)
            }
        };
    }

    private bool CanRefresh() => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        RefreshCommand.NotifyCanExecuteChanged();
    }
}