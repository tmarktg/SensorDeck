using DeviceSimulator;

const int port = 5050;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var server = new TcpTelemetryServer(port);

try
{
    await server.RunAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Simulator shutting down.");
}
