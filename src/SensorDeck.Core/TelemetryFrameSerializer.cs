using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorDeck.Core;

public static class TelemetryFrameSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static string SerializeFrame(TelemetryFrame frame) =>
        JsonSerializer.Serialize(frame, Options);

    public static TelemetryFrame? TryDeserializeFrame(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<TelemetryFrame>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string SerializeCommand(DeviceCommand command) =>
        JsonSerializer.Serialize(command, Options);

    public static DeviceCommand? TryDeserializeCommand(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<DeviceCommand>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
