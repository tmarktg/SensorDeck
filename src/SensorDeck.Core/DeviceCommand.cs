using System.Text.Json.Serialization;

namespace SensorDeck.Core;

public enum DeviceCommandKind
{
    ToggleHeater,
    SetSampleRate,
}

public record DeviceCommand(
    [property: JsonPropertyName("command")] DeviceCommandKind Command,
    [property: JsonPropertyName("value")] double? Value = null);
