using System.Threading.Channels;
using SensorDeck.Core;

namespace SensorDeck.Wpf.Services;

public sealed class TelemetryChannelReader : IAsyncDisposable
{
    private readonly DeviceConnection _connection = new();
    private readonly Channel<TelemetryFrame> _channel = Channel.CreateBounded<TelemetryFrame>(
        new BoundedChannelOptions(capacity: 256) { FullMode = BoundedChannelFullMode.DropOldest });

    private CancellationTokenSource? _producerCts;
    private Task? _producerTask;

    public bool IsConnected => _connection.IsConnected;

    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        await _connection.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);

        _producerCts = new CancellationTokenSource();
        _producerTask = ProduceAsync(_producerCts.Token);
    }

    private async Task ProduceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var frame in _connection.ReadFramesAsync(cancellationToken).ConfigureAwait(false))
            {
                await _channel.Writer.WriteAsync(frame, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // expected on disconnect/shutdown
        }
        finally
        {
            _channel.Writer.TryComplete();
        }
    }

    public IAsyncEnumerable<TelemetryFrame> ConsumeAsync(CancellationToken cancellationToken = default) =>
        _channel.Reader.ReadAllAsync(cancellationToken);

    public Task SendCommandAsync(DeviceCommand command, CancellationToken cancellationToken = default) =>
        _connection.SendCommandAsync(command, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        _producerCts?.Cancel();

        if (_producerTask is not null)
        {
            try
            {
                await _producerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }

        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}
