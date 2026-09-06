using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of a <c>PostProcessingListRequest</c>: lists post-processing effects and state.</summary>
public sealed record PostProcessingListRequest
{
    /// <summary>True fills the preset name array; reading presets can be slow.</summary>
    [JsonPropertyName("fillPostProcessingPresetsArray")] public bool FillPostProcessingPresetsArray { get; init; }

    /// <summary>True fills the effects array; the payload can be large.</summary>
    [JsonPropertyName("fillPostProcessingEffectsArray")] public bool FillPostProcessingEffectsArray { get; init; }

    /// <summary>Effect ids to include; empty applies no filter.</summary>
    [JsonPropertyName("effectIDFilter")] public IReadOnlyList<string> EffectIdFilter { get; init; } = [];
}

/// <summary>Payload of a <c>PostProcessingListResponse</c>: post-processing effects and state.</summary>
public sealed record PostProcessingListResponse
{
    /// <summary>True when the platform supports post-processing.</summary>
    [JsonPropertyName("postProcessingSupported")] public bool PostProcessingSupported { get; init; }

    /// <summary>True when post-processing is turned on.</summary>
    [JsonPropertyName("postProcessingActive")] public bool PostProcessingActive { get; init; }

    /// <summary>True when an update request can be sent right now.</summary>
    [JsonPropertyName("canSendPostProcessingUpdateRequestRightNow")] public bool CanSendPostProcessingUpdateRequestRightNow { get; init; }

    /// <summary>True when the user allowed restricted effects.</summary>
    [JsonPropertyName("restrictedEffectsAllowed")] public bool RestrictedEffectsAllowed { get; init; }

    /// <summary>True when a preset is active.</summary>
    [JsonPropertyName("presetIsActive")] public bool PresetIsActive { get; init; }

    /// <summary>The active preset name.</summary>
    [JsonPropertyName("activePreset")] public string? ActivePreset { get; init; }

    /// <summary>Number of presets in <see cref="PostProcessingPresets"/>.</summary>
    [JsonPropertyName("presetCount")] public int PresetCount { get; init; }

    /// <summary>Number of active effects.</summary>
    [JsonPropertyName("activeEffectCount")] public int ActiveEffectCount { get; init; }

    /// <summary>Effect count before filtering.</summary>
    [JsonPropertyName("effectCountBeforeFilter")] public int EffectCountBeforeFilter { get; init; }

    /// <summary>Config count before filtering.</summary>
    [JsonPropertyName("configCountBeforeFilter")] public int ConfigCountBeforeFilter { get; init; }

    /// <summary>Effect count after filtering.</summary>
    [JsonPropertyName("effectCountAfterFilter")] public int EffectCountAfterFilter { get; init; }

    /// <summary>Config count after filtering.</summary>
    [JsonPropertyName("configCountAfterFilter")] public int ConfigCountAfterFilter { get; init; }

    /// <summary>The effects.</summary>
    [JsonPropertyName("postProcessingEffects")] public IReadOnlyList<PostProcessingEffect> PostProcessingEffects { get; init; } = [];

    /// <summary>The preset names.</summary>
    [JsonPropertyName("postProcessingPresets")] public IReadOnlyList<string> PostProcessingPresets { get; init; } = [];
}

/// <summary>One effect in <see cref="PostProcessingListResponse.PostProcessingEffects"/>.</summary>
public sealed record PostProcessingEffect
{
    /// <summary>The internal id used in preset files.</summary>
    [JsonPropertyName("internalID")] public string? InternalId { get; init; }

    /// <summary>The id used to refer to the effect in the API.</summary>
    [JsonPropertyName("enumID")] public string? EnumId { get; init; }

    /// <summary>Description of the effect.</summary>
    [JsonPropertyName("explanation")] public string? Explanation { get; init; }

    /// <summary>True when the effect is currently active.</summary>
    [JsonPropertyName("effectIsActive")] public bool EffectIsActive { get; init; }

    /// <summary>True when the effect is restricted.</summary>
    [JsonPropertyName("effectIsRestricted")] public bool EffectIsRestricted { get; init; }

    /// <summary>The configs of the effect.</summary>
    [JsonPropertyName("configEntries")] public IReadOnlyList<PostProcessingEffectConfig> ConfigEntries { get; init; } = [];
}

/// <summary>One config entry of a <see cref="PostProcessingEffect"/>. Only the fields matching <see cref="Type"/> carry values.</summary>
public sealed record PostProcessingEffectConfig
{
    /// <summary>The internal id used in preset files.</summary>
    [JsonPropertyName("internalID")] public string? InternalId { get; init; }

    /// <summary>The id used to refer to the config in the API.</summary>
    [JsonPropertyName("enumID")] public string? EnumId { get; init; }

    /// <summary>Description of the config.</summary>
    [JsonPropertyName("explanation")] public string? Explanation { get; init; }

    /// <summary>Config type: <c>Float</c>, <c>Int</c>, <c>Bool</c>, <c>String</c>, <c>Color</c> or <c>SceneItem</c>.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }

    /// <summary>True when a value above zero activates the effect.</summary>
    [JsonPropertyName("activationConfig")] public bool ActivationConfig { get; init; }

    /// <summary>Float value.</summary>
    [JsonPropertyName("floatValue")] public double FloatValue { get; init; }

    /// <summary>Float minimum.</summary>
    [JsonPropertyName("floatMin")] public double FloatMin { get; init; }

    /// <summary>Float maximum.</summary>
    [JsonPropertyName("floatMax")] public double FloatMax { get; init; }

    /// <summary>Float default.</summary>
    [JsonPropertyName("floatDefault")] public double FloatDefault { get; init; }

    /// <summary>Int value.</summary>
    [JsonPropertyName("intValue")] public int IntValue { get; init; }

    /// <summary>Int minimum.</summary>
    [JsonPropertyName("intMin")] public int IntMin { get; init; }

    /// <summary>Int maximum.</summary>
    [JsonPropertyName("intMax")] public int IntMax { get; init; }

    /// <summary>Int default.</summary>
    [JsonPropertyName("intDefault")] public int IntDefault { get; init; }

    /// <summary>Color value as RGBA hex.</summary>
    [JsonPropertyName("colorValue")] public string? ColorValue { get; init; }

    /// <summary>Color default as RGBA hex.</summary>
    [JsonPropertyName("colorDefault")] public string? ColorDefault { get; init; }

    /// <summary>True when the color carries alpha.</summary>
    [JsonPropertyName("colorHasAlpha")] public bool ColorHasAlpha { get; init; }

    /// <summary>Bool value.</summary>
    [JsonPropertyName("boolValue")] public bool BoolValue { get; init; }

    /// <summary>Bool default.</summary>
    [JsonPropertyName("boolDefault")] public bool BoolDefault { get; init; }

    /// <summary>String value.</summary>
    [JsonPropertyName("stringValue")] public string? StringValue { get; init; }

    /// <summary>String default.</summary>
    [JsonPropertyName("stringDefault")] public string? StringDefault { get; init; }

    /// <summary>Scene item value.</summary>
    [JsonPropertyName("sceneItemValue")] public string? SceneItemValue { get; init; }

    /// <summary>Scene item default.</summary>
    [JsonPropertyName("sceneItemDefault")] public string? SceneItemDefault { get; init; }
}

/// <summary>Payload of a <c>PostProcessingUpdateRequest</c>: changes post-processing effects.</summary>
public sealed record PostProcessingUpdateRequest
{
    /// <summary>True turns post-processing on; false turns it off.</summary>
    [JsonPropertyName("postProcessingOn")] public bool PostProcessingOn { get; init; }

    /// <summary>True loads the preset in <see cref="PresetToSet"/>.</summary>
    [JsonPropertyName("setPostProcessingPreset")] public bool SetPostProcessingPreset { get; init; }

    /// <summary>True applies <see cref="PostProcessingValues"/>.</summary>
    [JsonPropertyName("setPostProcessingValues")] public bool SetPostProcessingValues { get; init; }

    /// <summary>Preset name without file extension.</summary>
    [JsonPropertyName("presetToSet")] public string? PresetToSet { get; init; }

    /// <summary>Fade duration in seconds (0-2).</summary>
    [JsonPropertyName("postProcessingFadeTime")] public double PostProcessingFadeTime { get; init; }

    /// <summary>True resets unmentioned values to their defaults.</summary>
    [JsonPropertyName("setAllOtherValuesToDefault")] public bool SetAllOtherValuesToDefault { get; init; }

    /// <summary>True allows restricted effects.</summary>
    [JsonPropertyName("usingRestrictedEffects")] public bool UsingRestrictedEffects { get; init; }

    /// <summary>True randomizes all configs, ignoring values and presets.</summary>
    [JsonPropertyName("randomizeAll")] public bool RandomizeAll { get; init; }

    /// <summary>Randomization chaos level (0-1).</summary>
    [JsonPropertyName("randomizeAllChaosLevel")] public double RandomizeAllChaosLevel { get; init; }

    /// <summary>Config values to set.</summary>
    [JsonPropertyName("postProcessingValues")] public IReadOnlyList<PostProcessingValue> PostProcessingValues { get; init; } = [];
}

/// <summary>One config value in <see cref="PostProcessingUpdateRequest.PostProcessingValues"/>.</summary>
public sealed record PostProcessingValue
{
    /// <summary>The config id.</summary>
    [JsonPropertyName("configID")] public required string ConfigId { get; init; }

    /// <summary>The value to set, as string.</summary>
    [JsonPropertyName("configValue")] public required string ConfigValue { get; init; }
}

/// <summary>Payload of a <c>PostProcessingUpdateResponse</c>: the post-processing state after the update.</summary>
public sealed record PostProcessingUpdateResponse
{
    /// <summary>True when post-processing is active.</summary>
    [JsonPropertyName("postProcessingActive")] public bool PostProcessingActive { get; init; }

    /// <summary>True when a preset is active.</summary>
    [JsonPropertyName("presetIsActive")] public bool PresetIsActive { get; init; }

    /// <summary>The active preset name.</summary>
    [JsonPropertyName("activePreset")] public string? ActivePreset { get; init; }

    /// <summary>Number of active effects.</summary>
    [JsonPropertyName("activeEffectCount")] public int ActiveEffectCount { get; init; }
}
