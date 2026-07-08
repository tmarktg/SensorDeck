using SensorDeck.Core;

namespace DeviceSimulator;

public sealed class TelemetrySource
{
    private readonly Random _random = new();
    private double _temperature = 22.0;
    private double _voltage = 12.0;
    private double _vibration = 0.05;

    public bool HeaterOn { get; set; }
    public int SampleRateMs { get; set; } = 100;

    public TelemetryFrame NextFrame()
    {
        var heaterBias = HeaterOn ? 0.15 : -0.05;
        _temperature += heaterBias + ((_random.NextDouble() - 0.5) * 0.3);
        _temperature = Math.Clamp(_temperature, 15, 90);

        _voltage += (_random.NextDouble() - 0.5) * 0.05;
        _voltage = Math.Clamp(_voltage, 11.5, 12.5);

        var vibrationDrift = (_random.NextDouble() - 0.5) * 0.02 + (HeaterOn ? 0.01 : 0);
        _vibration = Math.Max(0, _vibration + vibrationDrift);

        return new TelemetryFrame(
            DateTimeOffset.UtcNow,
            Math.Round(_temperature, 2),
            Math.Round(_voltage, 3),
            Math.Round(_vibration, 3));
    }
}
