using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of a <c>HotkeysInCurrentModelRequest</c>: asks for the hotkeys of the current or a specific model/item.</summary>
public sealed record HotkeysInCurrentModelRequest
{
    /// <summary>Optional - query hotkeys of a specific model file even if it isn't loaded.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>Optional - query hotkeys of a Live2D item by its file name instead of a model.</summary>
    [JsonPropertyName("live2DItemFileName")] public string? Live2DItemFileName { get; init; }
}

/// <summary>Payload of a <c>HotkeysInCurrentModelResponse</c>: the hotkeys available for the queried model.</summary>
public sealed record HotkeysInCurrentModelResponse
{
    /// <summary>True when a model is loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The queried model's display name.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>The queried model's unique identifier.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The hotkeys defined for the model.</summary>
    [JsonPropertyName("availableHotkeys")] public IReadOnlyList<AvailableHotkey> AvailableHotkeys { get; init; } = [];
}

/// <summary>An entry in <see cref="HotkeysInCurrentModelResponse.AvailableHotkeys"/> describing one hotkey.</summary>
public sealed record AvailableHotkey
{
    /// <summary>The hotkey's display name/label.</summary>
    [JsonPropertyName("name")] public required string Name { get; init; }

    /// <summary>The hotkey action type (for example <c>ToggleExpression</c>, <c>TriggerAnimation</c>).</summary>
    [JsonPropertyName("type")] public required string Type { get; init; }

    /// <summary>A description of what the hotkey does.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }

    /// <summary>The file the hotkey operates on (for example the expression or animation file), when applicable.</summary>
    [JsonPropertyName("file")] public string? File { get; init; }

    /// <summary>The hotkey's unique identifier, used to trigger it via <see cref="HotkeyTriggerRequest"/>.</summary>
    [JsonPropertyName("hotkeyID")] public required string HotkeyId { get; init; }

    /// <summary>The key binding for the hotkey (currently always empty in the API).</summary>
    [JsonPropertyName("keyCombination")] public IReadOnlyList<string> KeyCombination { get; init; } = [];

    /// <summary>The on-screen button number (1-8) the hotkey is mapped to, or -1 when it is not mapped to a button.</summary>
    [JsonPropertyName("onScreenButtonID")] public int OnScreenButtonId { get; init; }
}

/// <summary>Payload of a <c>HotkeyTriggerRequest</c>: executes a hotkey by id or name.</summary>
public sealed record HotkeyTriggerRequest
{
    /// <summary>The hotkey id or name to trigger (matching is case-insensitive).</summary>
    [JsonPropertyName("hotkeyID")] public required string HotkeyId { get; init; }

    /// <summary>Optional - the instance id of a Live2D item to trigger the hotkey on instead of the main model.</summary>
    [JsonPropertyName("itemInstanceID")] public string? ItemInstanceId { get; init; }
}

/// <summary>Payload of a <c>HotkeyTriggerResponse</c>: confirms which hotkey was triggered.</summary>
public sealed record HotkeyTriggerResponse
{
    /// <summary>The id of the hotkey that was triggered.</summary>
    [JsonPropertyName("hotkeyID")] public required string HotkeyId { get; init; }
}
