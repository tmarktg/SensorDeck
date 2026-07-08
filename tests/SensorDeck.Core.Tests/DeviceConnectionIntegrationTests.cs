using DeviceSimulator;
using SensorDeck.Core;
using Xunit;

namespace SensorDeck.Core.Tests;

public class DeviceConnectionIntegrationTests
{
    [Fact]
    public async Task Client_Receives_Frames_From_Simulator()
    {
        var server = new TcpTelemetryServer(port: 0);
        using var serverCts = new CancellationTokenSource();
        var serverTask = server.RunAsync(serverCts.Token);

        await using var connection = new DeviceConnection();
        await connection.ConnectAsync("127.0.0.1", server.Port);

        using var readCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var frames = new List<TelemetryFrame>();

        await foreach (var frame in connection.ReadFramesAsync(readCts.Token))
        {
            frames.Add(frame);
            if (frames.Count >= 3)
                break;
        }

        Assert.True(frames.Count >= 3);
        Assert.All(frames, f => Assert.InRange(f.Temperature, -50, 200));

        serverCts.Cancel();
        try
        {
            await serverTask;
        }
        catch (OperationCanceledException)
        {
            // expected shutdown
        }
    }

    [Fact]
    public async Task Command_Sent_By_Client_Changes_Simulator_Output()
    {
        var server = new TcpTelemetryServer(port: 0);
        using var serverCts = new CancellationTokenSource();
        var serverTask = server.RunAsync(serverCts.Token);

        await using var connection = new DeviceConnection();
        await connection.ConnectAsync("127.0.0.1", server.Port);

        await connection.SendCommandAsync(new DeviceCommand(DeviceCommandKind.SetSampleRate, 20));

        using var readCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var frames = new List<TelemetryFrame>();

        await foreach (var frame in connection.ReadFramesAsync(readCts.Token))
        {
            frames.Add(frame);
            if (frames.Count >= 5)
                break;
        }

        Assert.True(frames.Count >= 5);

        serverCts.Cancel();
        try
        {
            await serverTask;
        }
        catch (OperationCanceledException)
        {
            // expected shutdown
        }
    }
}
