using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace SensorDeck.Core;

public sealed class DeviceConnection : IAsyncDisposable
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public bool IsConnected => _client?.Connected ?? false;

    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        var client = new TcpClient();
        await client.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);

        _client = client;
        _stream = client.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8);
        _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
    }

    public async IAsyncEnumerable<TelemetryFrame> ReadFramesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_reader is null)
            throw new InvalidOperationException("Not connected. Call ConnectAsync first.");

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
                yield break;

            var frame = TelemetryFrameSerializer.TryDeserializeFrame(line);
            if (frame is not null)
                yield return frame;
        }
    }

    public async Task SendCommandAsync(DeviceCommand command, CancellationToken cancellationToken = default)
    {
        if (_writer is null)
            throw new InvalidOperationException("Not connected. Call ConnectAsync first.");

        var json = TelemetryFrameSerializer.SerializeCommand(command);
        await _writer.WriteLineAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        if (_stream is not null)
            await _stream.DisposeAsync().ConfigureAwait(false);
        _client?.Dispose();
    }
}
