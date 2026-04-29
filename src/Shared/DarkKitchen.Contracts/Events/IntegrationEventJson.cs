using System.Text.Json;

namespace DarkKitchen.Contracts.Events;

public static class IntegrationEventJson
{
    public const int CurrentSchemaVersion = 1;

    public static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };
}
