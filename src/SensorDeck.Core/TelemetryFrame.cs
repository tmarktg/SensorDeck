using System.Text.Json.Serialization;

namespace SensorDeck.Core;

public record TelemetryFrame(
    [property: JsonPropertyName("ts")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("voltage")] double Voltage,
    [property: JsonPropertyName("vibration")] double Vibration);
