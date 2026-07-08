using System.Net;
using System.Net.Sockets;
using System.Text;
using SensorDeck.Core;

namespace DeviceSimulator;

public sealed class TcpTelemetryServer
{
    private readonly TelemetrySource _source = new();
    private readonly TcpListener _listener;

    public TcpTelemetryServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        Console.WriteLine($"DeviceSimulator listening on port {Port}");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        finally
        {
            _listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using var _ = client;
        await using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        await using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        var readTask = ReadCommandsAsync(reader, cancellationToken);
        var writeTask = WriteFramesAsync(writer, cancellationToken);

        await Task.WhenAny(readTask, writeTask).ConfigureAwait(false);
        Console.WriteLine("Client disconnected.");
    }

    private async Task ReadCommandsAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
                return;

            var command = TelemetryFrameSerializer.TryDeserializeCommand(line);
            if (command is null)
                continue;

            switch (command.Command)
            {
                case DeviceCommandKind.ToggleHeater:
                    _source.HeaterOn = !_source.HeaterOn;
                    Console.WriteLine($"Heater toggled -> {_source.HeaterOn}");
                    break;
                case DeviceCommandKind.SetSampleRate:
                    if (command.Value is > 0)
                    {
                        _source.SampleRateMs = (int)command.Value.Value;
                        Console.WriteLine($"Sample rate set -> {_source.SampleRateMs}ms");
                    }
                    break;
            }
        }
    }

    private async Task WriteFramesAsync(StreamWriter writer, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var frame = _source.NextFrame();
            var json = TelemetryFrameSerializer.SerializeFrame(frame);
            await writer.WriteLineAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);
            await Task.Delay(_source.SampleRateMs, cancellationToken).ConfigureAwait(false);
        }
    }
}
