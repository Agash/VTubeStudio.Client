using System.Text.Json.Serialization;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;

namespace VTubeStudio.Client.Serialization;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for the VTube Studio wire types.
/// Keeps the library AOT- and trim-friendly: no reflection-driven serialisation at runtime.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(VTubeStudioEnvelope))]
[JsonSerializable(typeof(ApiErrorData))]
[JsonSerializable(typeof(ApiStateResponse))]
[JsonSerializable(typeof(StatisticsResponse))]
[JsonSerializable(typeof(FaceFoundResponse))]
// Authentication
[JsonSerializable(typeof(AuthenticationTokenRequest))]
[JsonSerializable(typeof(AuthenticationTokenResponse))]
[JsonSerializable(typeof(AuthenticationRequest))]
[JsonSerializable(typeof(AuthenticationResponse))]
// Models
[JsonSerializable(typeof(CurrentModelResponse))]
[JsonSerializable(typeof(AvailableModelsResponse))]
[JsonSerializable(typeof(AvailableModel))]
[JsonSerializable(typeof(ModelLoadRequest))]
[JsonSerializable(typeof(ModelLoadResponse))]
[JsonSerializable(typeof(MoveModelRequest))]
// Hotkeys + expressions
[JsonSerializable(typeof(HotkeysInCurrentModelRequest))]
[JsonSerializable(typeof(HotkeysInCurrentModelResponse))]
[JsonSerializable(typeof(AvailableHotkey))]
[JsonSerializable(typeof(HotkeyTriggerRequest))]
[JsonSerializable(typeof(HotkeyTriggerResponse))]
[JsonSerializable(typeof(ExpressionStateRequest))]
[JsonSerializable(typeof(ExpressionStateResponse))]
[JsonSerializable(typeof(ExpressionInfo))]
[JsonSerializable(typeof(ExpressionActivationRequest))]
// Parameters
[JsonSerializable(typeof(InputParameterListResponse))]
[JsonSerializable(typeof(Live2DParameterListResponse))]
[JsonSerializable(typeof(ParameterInfo))]
[JsonSerializable(typeof(ParameterValueRequest))]
[JsonSerializable(typeof(InjectParameterDataRequest))]
[JsonSerializable(typeof(ParameterValue))]
// ArtMesh
[JsonSerializable(typeof(ArtMeshListResponse))]
[JsonSerializable(typeof(ColorTintRequest))]
[JsonSerializable(typeof(ColorTint))]
[JsonSerializable(typeof(ArtMeshMatcher))]
// Items
[JsonSerializable(typeof(ItemListRequest))]
[JsonSerializable(typeof(ItemListResponse))]
[JsonSerializable(typeof(ItemInstance))]
[JsonSerializable(typeof(AvailableItemFile))]
[JsonSerializable(typeof(ItemLoadRequest))]
[JsonSerializable(typeof(ItemLoadResponse))]
[JsonSerializable(typeof(ItemUnloadRequest))]
[JsonSerializable(typeof(ItemUnloadResponse))]
// Events
[JsonSerializable(typeof(EventSubscriptionRequest))]
[JsonSerializable(typeof(EventSubscriptionResponse))]
[JsonSerializable(typeof(TestEventConfig))]
[JsonSerializable(typeof(ModelLoadedEventConfig))]
[JsonSerializable(typeof(ModelOutlineEventConfig))]
[JsonSerializable(typeof(HotkeyTriggeredEventConfig))]
[JsonSerializable(typeof(ModelAnimationEventConfig))]
[JsonSerializable(typeof(ItemEventConfig))]
[JsonSerializable(typeof(ModelClickedEventConfig))]
[JsonSerializable(typeof(ModelLoadedEventPayload))]
[JsonSerializable(typeof(TrackingStatusChangedEventPayload))]
[JsonSerializable(typeof(BackgroundChangedEventPayload))]
[JsonSerializable(typeof(ModelConfigChangedEventPayload))]
[JsonSerializable(typeof(ModelMovedEventPayload))]
[JsonSerializable(typeof(ModelPosition))]
[JsonSerializable(typeof(HotkeyTriggeredEventPayload))]
[JsonSerializable(typeof(ModelAnimationEventPayload))]
[JsonSerializable(typeof(ItemEventPayload))]
[JsonSerializable(typeof(ModelClickedEventPayload))]
[JsonSerializable(typeof(ClickPosition))]
[JsonSerializable(typeof(WindowSize))]
[JsonSerializable(typeof(PostProcessingEventPayload))]
public sealed partial class VTubeStudioJsonContext : JsonSerializerContext;
