using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

public sealed record HotkeysInCurrentModelRequest
{
    /// <summary>Optional — query hotkeys of a specific model file even if it isn't loaded.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("live2DItemFileName")] public string? Live2DItemFileName { get; init; }
}

public sealed record HotkeysInCurrentModelResponse
{
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("availableHotkeys")] public IReadOnlyList<AvailableHotkey> AvailableHotkeys { get; init; } = [];
}

public sealed record AvailableHotkey
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("type")] public required string Type { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
    [JsonPropertyName("file")] public string? File { get; init; }
    [JsonPropertyName("hotkeyID")] public required string HotkeyId { get; init; }
    [JsonPropertyName("keyCombination")] public IReadOnlyList<string> KeyCombination { get; init; } = [];
    [JsonPropertyName("onScreenButtonID")] public int OnScreenButtonId { get; init; }
}

public sealed record HotkeyTriggerRequest
{
    [JsonPropertyName("hotkeyID")] public required string HotkeyId { get; init; }
    [JsonPropertyName("itemInstanceID")] public string? ItemInstanceId { get; init; }
}

public sealed record HotkeyTriggerResponse
{
    [JsonPropertyName("hotkeyID")] public required string HotkeyId { get; init; }
}
