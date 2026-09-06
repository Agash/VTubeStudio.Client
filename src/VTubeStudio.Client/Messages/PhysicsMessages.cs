using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of a <c>GetCurrentModelPhysicsResponse</c>: the physics settings of the loaded model.</summary>
public sealed record GetCurrentModelPhysicsResponse
{
    /// <summary>True when a model is loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The model's display name.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>The model's unique identifier.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>True when the model has a valid physics setup.</summary>
    [JsonPropertyName("modelHasPhysics")] public bool ModelHasPhysics { get; init; }

    /// <summary>True when the user enabled physics for the model.</summary>
    [JsonPropertyName("physicsSwitchedOn")] public bool PhysicsSwitchedOn { get; init; }

    /// <summary>True when the legacy physics toggle is on.</summary>
    [JsonPropertyName("usingLegacyPhysics")] public bool UsingLegacyPhysics { get; init; }

    /// <summary>Physics FPS setting; -1 means same FPS as the app.</summary>
    [JsonPropertyName("physicsFPSSetting")] public int PhysicsFpsSetting { get; init; }

    /// <summary>Base physics strength (0-100).</summary>
    [JsonPropertyName("baseStrength")] public int BaseStrength { get; init; }

    /// <summary>Base wind strength (0-100).</summary>
    [JsonPropertyName("baseWind")] public int BaseWind { get; init; }

    /// <summary>True when a plugin currently overrides physics settings.</summary>
    [JsonPropertyName("apiPhysicsOverrideActive")] public bool ApiPhysicsOverrideActive { get; init; }

    /// <summary>The name of the overriding plugin.</summary>
    [JsonPropertyName("apiPhysicsOverridePluginName")] public string? ApiPhysicsOverridePluginName { get; init; }

    /// <summary>The physics groups of the model.</summary>
    [JsonPropertyName("physicsGroups")] public IReadOnlyList<PhysicsGroup> PhysicsGroups { get; init; } = [];
}

/// <summary>A physics group in <see cref="GetCurrentModelPhysicsResponse.PhysicsGroups"/>.</summary>
public sealed record PhysicsGroup
{
    /// <summary>The group id.</summary>
    [JsonPropertyName("groupID")] public string? GroupId { get; init; }

    /// <summary>The group name; may be empty.</summary>
    [JsonPropertyName("groupName")] public string? GroupName { get; init; }

    /// <summary>Strength multiplier (0-2).</summary>
    [JsonPropertyName("strengthMultiplier")] public double StrengthMultiplier { get; init; }

    /// <summary>Wind multiplier (0-2).</summary>
    [JsonPropertyName("windMultiplier")] public double WindMultiplier { get; init; }
}

/// <summary>Payload of a <c>SetCurrentModelPhysicsRequest</c>: temporarily overrides physics settings.</summary>
public sealed record SetCurrentModelPhysicsRequest
{
    /// <summary>Strength overrides.</summary>
    [JsonPropertyName("strengthOverrides")] public IReadOnlyList<PhysicsOverride> StrengthOverrides { get; init; } = [];

    /// <summary>Wind overrides.</summary>
    [JsonPropertyName("windOverrides")] public IReadOnlyList<PhysicsOverride> WindOverrides { get; init; } = [];
}

/// <summary>One override in <see cref="SetCurrentModelPhysicsRequest"/>.</summary>
public sealed record PhysicsOverride
{
    /// <summary>The physics group id; empty with <see cref="SetBaseValue"/> sets the base value.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }

    /// <summary>The override value.</summary>
    [JsonPropertyName("value")] public double Value { get; init; }

    /// <summary>True to set the base value instead of a group multiplier.</summary>
    [JsonPropertyName("setBaseValue")] public bool SetBaseValue { get; init; }

    /// <summary>Seconds the override lasts (0.5-5).</summary>
    [JsonPropertyName("overrideSeconds")] public double OverrideSeconds { get; init; }
}
