using SensorDeck.Core;
using Xunit;

namespace SensorDeck.Core.Tests;

public class TelemetryFrameSerializerTests
{
    [Fact]
    public void TryDeserializeFrame_ValidJson_ReturnsFrame()
    {
        const string json = """{"ts":"2026-07-08T12:00:00Z","temperature":21.5,"voltage":12.1,"vibration":0.05}""";

        var frame = TelemetryFrameSerializer.TryDeserializeFrame(json);

        Assert.NotNull(frame);
        Assert.Equal(21.5, frame!.Temperature);
        Assert.Equal(12.1, frame.Voltage);
        Assert.Equal(0.05, frame.Vibration);
    }

    [Fact]
    public void TryDeserializeFrame_MalformedJson_ReturnsNull()
    {
        var result = TelemetryFrameSerializer.TryDeserializeFrame("not json{{{");

        Assert.Null(result);
    }

    [Fact]
    public void SerializeFrame_RoundTrips()
    {
        var original = new TelemetryFrame(DateTimeOffset.UtcNow, 30.1, 12.4, 0.12);

        var json = TelemetryFrameSerializer.SerializeFrame(original);
        var roundTripped = TelemetryFrameSerializer.TryDeserializeFrame(json);

        Assert.Equal(original, roundTripped);
    }

    [Fact]
    public void SerializeCommand_RoundTrips_WithEnumAsString()
    {
        var command = new DeviceCommand(DeviceCommandKind.SetSampleRate, 250);

        var json = TelemetryFrameSerializer.SerializeCommand(command);

        Assert.Contains("\"SetSampleRate\"", json);

        var roundTripped = TelemetryFrameSerializer.TryDeserializeCommand(json);
        Assert.Equal(command, roundTripped);
    }

    [Fact]
    public void TryDeserializeCommand_MalformedJson_ReturnsNull()
    {
        var result = TelemetryFrameSerializer.TryDeserializeCommand("{not valid");

        Assert.Null(result);
    }
}
