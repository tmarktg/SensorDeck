using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SensorDeck.Core;
using SensorDeck.Wpf.Services;

namespace SensorDeck.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject, IAsyncDisposable
{
    private const string DefaultHost = "127.0.0.1";
    private const int DefaultPort = 5050;
    private const int MaxPoints = 200;

    private readonly TelemetryChannelReader _channelReader = new();
    private readonly ObservableCollection<double> _temperatureValues = new();

    private CancellationTokenSource? _consumeCts;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleHeaterCommand))]
    [NotifyCanExecuteChangedFor(nameof(ApplySampleRateCommand))]
    private bool isConnected;

    [ObservableProperty]
    private double currentTemperature;

    [ObservableProperty]
    private double currentVoltage;

    [ObservableProperty]
    private double currentVibration;

    [ObservableProperty]
    private bool heaterOn;

    [ObservableProperty]
    private double sampleRateMs = 100;

    [ObservableProperty]
    private string statusMessage = "Disconnected";

    public ISeries[] TemperatureSeries { get; }

    public MainViewModel()
    {
        TemperatureSeries =
        [
            new LineSeries<double>
            {
                Values = _temperatureValues,
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0.3,
            }
        ];
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        StatusMessage = "Connecting...";

        try
        {
            await _channelReader.ConnectAsync(DefaultHost, DefaultPort).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                IsConnected = true;
                StatusMessage = $"Connected to {DefaultHost}:{DefaultPort}";
            });

            _consumeCts = new CancellationTokenSource();
            _ = ConsumeFramesAsync(_consumeCts.Token);
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StatusMessage = $"Connection failed: {ex.Message}";
            });
        }
    }

    private bool CanConnect() => !IsConnected;

    private async Task ConsumeFramesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var frame in _channelReader.ConsumeAsync(cancellationToken).ConfigureAwait(false))
            {
                await Application.Current.Dispatcher.InvokeAsync(() => ApplyFrame(frame));
            }
        }
        catch (OperationCanceledException)
        {
            // expected on disconnect/shutdown
        }
        finally
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                IsConnected = false;
                StatusMessage = "Disconnected";
            });
        }
    }

    private void ApplyFrame(TelemetryFrame frame)
    {
        CurrentTemperature = frame.Temperature;
        CurrentVoltage = frame.Voltage;
        CurrentVibration = frame.Vibration;

        _temperatureValues.Add(frame.Temperature);
        if (_temperatureValues.Count > MaxPoints)
            _temperatureValues.RemoveAt(0);
    }

    [RelayCommand(CanExecute = nameof(IsConnected))]
    private async Task ToggleHeaterAsync()
    {
        HeaterOn = !HeaterOn;
        await _channelReader.SendCommandAsync(new DeviceCommand(DeviceCommandKind.ToggleHeater)).ConfigureAwait(false);
    }

    [RelayCommand(CanExecute = nameof(IsConnected))]
    private async Task ApplySampleRateAsync()
    {
        await _channelReader
            .SendCommandAsync(new DeviceCommand(DeviceCommandKind.SetSampleRate, SampleRateMs))
            .ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        _consumeCts?.Cancel();
        await _channelReader.DisposeAsync().ConfigureAwait(false);
    }
}
