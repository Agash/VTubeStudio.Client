namespace VTubeStudio.Client.Messages;

/// <summary>Well-known <c>messageType</c> values used by the VTube Studio Public API.</summary>
public static class VTubeStudioMessageTypes
{
    // Session / state ------------------------------------------------------

    /// <summary><c>messageType</c> for a request asking whether the API is active and the session authenticated.</summary>
    public const string ApiStateRequest = "APIStateRequest";

    /// <summary><c>messageType</c> of the response carrying API active state, VTS version, and session auth status.</summary>
    public const string ApiStateResponse = "APIStateResponse";

    /// <summary><c>messageType</c> for a request asking for VTube Studio runtime statistics.</summary>
    public const string StatisticsRequest = "StatisticsRequest";

    /// <summary><c>messageType</c> of the response carrying uptime, framerate, plugin counts, and window metrics.</summary>
    public const string StatisticsResponse = "StatisticsResponse";

    /// <summary><c>messageType</c> for a request asking for the names of VTube Studio's data folders.</summary>
    public const string VtsFolderInfoRequest = "VTSFolderInfoRequest";

    /// <summary><c>messageType</c> of the response carrying the names of the models, backgrounds, items, config, logs, and backup folders.</summary>
    public const string VtsFolderInfoResponse = "VTSFolderInfoResponse";

    // Authentication --------------------------------------------------------

    /// <summary><c>messageType</c> for a request asking the user to grant a fresh authentication token.</summary>
    public const string AuthenticationTokenRequest = "AuthenticationTokenRequest";

    /// <summary><c>messageType</c> of the response carrying the granted authentication token.</summary>
    public const string AuthenticationTokenResponse = "AuthenticationTokenResponse";

    /// <summary><c>messageType</c> for a request authenticating the current session with a previously granted token.</summary>
    public const string AuthenticationRequest = "AuthenticationRequest";

    /// <summary><c>messageType</c> of the response reporting whether the session is now authenticated.</summary>
    public const string AuthenticationResponse = "AuthenticationResponse";

    /// <summary><c>messageType</c> for a request asking for a permission or listing granted permissions.</summary>
    public const string PermissionRequest = "PermissionRequest";

    /// <summary><c>messageType</c> of the response carrying the grant result and the permission list.</summary>
    public const string PermissionResponse = "PermissionResponse";

    // Models ----------------------------------------------------------------

    /// <summary><c>messageType</c> for a request asking for information about the currently loaded model.</summary>
    public const string CurrentModelRequest = "CurrentModelRequest";

    /// <summary><c>messageType</c> of the response describing the currently loaded model.</summary>
    public const string CurrentModelResponse = "CurrentModelResponse";

    /// <summary><c>messageType</c> for a request asking for the list of all models available on the machine.</summary>
    public const string AvailableModelsRequest = "AvailableModelsRequest";

    /// <summary><c>messageType</c> of the response listing all available models.</summary>
    public const string AvailableModelsResponse = "AvailableModelsResponse";

    /// <summary><c>messageType</c> for a request to load a model by its ID.</summary>
    public const string ModelLoadRequest = "ModelLoadRequest";

    /// <summary><c>messageType</c> of the response confirming which model was loaded.</summary>
    public const string ModelLoadResponse = "ModelLoadResponse";

    /// <summary><c>messageType</c> for a request to move, rotate, and scale the currently loaded model.</summary>
    public const string MoveModelRequest = "MoveModelRequest";

    /// <summary><c>messageType</c> of the response acknowledging a model-move request.</summary>
    public const string MoveModelResponse = "MoveModelResponse";

    // Hotkeys & expressions ------------------------------------------------

    /// <summary><c>messageType</c> for a request asking for the hotkeys available in a model.</summary>
    public const string HotkeysInCurrentModelRequest = "HotkeysInCurrentModelRequest";

    /// <summary><c>messageType</c> of the response listing the available hotkeys.</summary>
    public const string HotkeysInCurrentModelResponse = "HotkeysInCurrentModelResponse";

    /// <summary><c>messageType</c> for a request to execute (trigger) a hotkey.</summary>
    public const string HotkeyTriggerRequest = "HotkeyTriggerRequest";

    /// <summary><c>messageType</c> of the response confirming which hotkey was triggered.</summary>
    public const string HotkeyTriggerResponse = "HotkeyTriggerResponse";

    /// <summary><c>messageType</c> for a request asking for the activation state of the model's expressions.</summary>
    public const string ExpressionStateRequest = "ExpressionStateRequest";

    /// <summary><c>messageType</c> of the response listing expression states.</summary>
    public const string ExpressionStateResponse = "ExpressionStateResponse";

    /// <summary><c>messageType</c> for a request to activate or deactivate an expression.</summary>
    public const string ExpressionActivationRequest = "ExpressionActivationRequest";

    /// <summary><c>messageType</c> of the response acknowledging an expression activation request.</summary>
    public const string ExpressionActivationResponse = "ExpressionActivationResponse";

    // ArtMesh & tints -------------------------------------------------------

    /// <summary><c>messageType</c> for a request asking for the list of ArtMeshes and tags in the current model.</summary>
    public const string ArtMeshListRequest = "ArtMeshListRequest";

    /// <summary><c>messageType</c> of the response listing ArtMesh names and tags.</summary>
    public const string ArtMeshListResponse = "ArtMeshListResponse";

    /// <summary><c>messageType</c> for a request to apply a color tint to selected ArtMeshes.</summary>
    public const string ColorTintRequest = "ColorTintRequest";

    /// <summary><c>messageType</c> of the response reporting how many ArtMeshes were tinted.</summary>
    public const string ColorTintResponse = "ColorTintResponse";

    /// <summary><c>messageType</c> for a request asking for the ArtMeshes at a position.</summary>
    public const string ArtMeshAtPositionRequest = "ArtMeshAtPositionRequest";

    /// <summary><c>messageType</c> of the response listing the ArtMeshes at the checked position.</summary>
    public const string ArtMeshAtPositionResponse = "ArtMeshAtPositionResponse";

    /// <summary><c>messageType</c> for a request asking the user to select ArtMeshes.</summary>
    public const string ArtMeshSelectionRequest = "ArtMeshSelectionRequest";

    /// <summary><c>messageType</c> of the response carrying the user's ArtMesh selection.</summary>
    public const string ArtMeshSelectionResponse = "ArtMeshSelectionResponse";

    // Parameters ------------------------------------------------------------

    /// <summary><c>messageType</c> for a request asking for the list of available tracking input parameters.</summary>
    public const string InputParameterListRequest = "InputParameterListRequest";

    /// <summary><c>messageType</c> of the response listing default and custom tracking parameters.</summary>
    public const string InputParameterListResponse = "InputParameterListResponse";

    /// <summary><c>messageType</c> for a request asking for the model's Live2D parameters and their values.</summary>
    public const string Live2DParameterListRequest = "Live2DParameterListRequest";

    /// <summary><c>messageType</c> of the response listing the Live2D parameters.</summary>
    public const string Live2DParameterListResponse = "Live2DParameterListResponse";

    /// <summary><c>messageType</c> for a request asking for the current value of a single parameter.</summary>
    public const string ParameterValueRequest = "ParameterValueRequest";

    /// <summary><c>messageType</c> of the response carrying a single parameter's value and range.</summary>
    public const string ParameterValueResponse = "ParameterValueResponse";

    /// <summary><c>messageType</c> for a request feeding tracking data into one or more parameters.</summary>
    public const string InjectParameterDataRequest = "InjectParameterDataRequest";

    /// <summary><c>messageType</c> of the response acknowledging an inject-parameter-data request.</summary>
    public const string InjectParameterDataResponse = "InjectParameterDataResponse";

    /// <summary><c>messageType</c> for a request creating a custom tracking parameter.</summary>
    public const string ParameterCreationRequest = "ParameterCreationRequest";

    /// <summary><c>messageType</c> of the response confirming the created parameter.</summary>
    public const string ParameterCreationResponse = "ParameterCreationResponse";

    /// <summary><c>messageType</c> for a request deleting a custom tracking parameter.</summary>
    public const string ParameterDeletionRequest = "ParameterDeletionRequest";

    /// <summary><c>messageType</c> of the response confirming the deleted parameter.</summary>
    public const string ParameterDeletionResponse = "ParameterDeletionResponse";

    /// <summary><c>messageType</c> for a request asking for the scene lighting overlay state.</summary>
    public const string SceneColorOverlayInfoRequest = "SceneColorOverlayInfoRequest";

    /// <summary><c>messageType</c> of the response carrying the scene lighting overlay state.</summary>
    public const string SceneColorOverlayInfoResponse = "SceneColorOverlayInfoResponse";

    /// <summary><c>messageType</c> for a request asking for the current model physics settings.</summary>
    public const string GetCurrentModelPhysicsRequest = "GetCurrentModelPhysicsRequest";

    /// <summary><c>messageType</c> of the response carrying the physics settings.</summary>
    public const string GetCurrentModelPhysicsResponse = "GetCurrentModelPhysicsResponse";

    /// <summary><c>messageType</c> for a request overriding the current model physics settings.</summary>
    public const string SetCurrentModelPhysicsRequest = "SetCurrentModelPhysicsRequest";

    /// <summary><c>messageType</c> of the response acknowledging a physics override.</summary>
    public const string SetCurrentModelPhysicsResponse = "SetCurrentModelPhysicsResponse";

    /// <summary><c>messageType</c> for a request reading or changing the NDI configuration.</summary>
    public const string NdiConfigRequest = "NDIConfigRequest";

    /// <summary><c>messageType</c> of the response carrying the NDI configuration.</summary>
    public const string NdiConfigResponse = "NDIConfigResponse";

    /// <summary><c>messageType</c> for a request listing post-processing effects and state.</summary>
    public const string PostProcessingListRequest = "PostProcessingListRequest";

    /// <summary><c>messageType</c> of the response carrying post-processing effects and state.</summary>
    public const string PostProcessingListResponse = "PostProcessingListResponse";

    /// <summary><c>messageType</c> for a request changing post-processing effects.</summary>
    public const string PostProcessingUpdateRequest = "PostProcessingUpdateRequest";

    /// <summary><c>messageType</c> of the response confirming the post-processing update.</summary>
    public const string PostProcessingUpdateResponse = "PostProcessingUpdateResponse";

    // Items -----------------------------------------------------------------

    /// <summary><c>messageType</c> for a request asking for available item files and/or loaded item instances.</summary>
    public const string ItemListRequest = "ItemListRequest";

    /// <summary><c>messageType</c> of the response listing items in the scene and/or available item files.</summary>
    public const string ItemListResponse = "ItemListResponse";

    /// <summary><c>messageType</c> for a request to load an item into the scene.</summary>
    public const string ItemLoadRequest = "ItemLoadRequest";

    /// <summary><c>messageType</c> of the response confirming the loaded item's instance ID.</summary>
    public const string ItemLoadResponse = "ItemLoadResponse";

    /// <summary><c>messageType</c> for a request to unload one or more items from the scene.</summary>
    public const string ItemUnloadRequest = "ItemUnloadRequest";

    /// <summary><c>messageType</c> of the response listing the items that were unloaded.</summary>
    public const string ItemUnloadResponse = "ItemUnloadResponse";

    /// <summary><c>messageType</c> for a request to control playback/appearance of an animated item.</summary>
    public const string ItemAnimationControlRequest = "ItemAnimationControlRequest";

    /// <summary><c>messageType</c> of the response carrying the item's current frame and play state.</summary>
    public const string ItemAnimationControlResponse = "ItemAnimationControlResponse";

    /// <summary><c>messageType</c> for a request to move one or more items in the scene.</summary>
    public const string ItemMoveRequest = "ItemMoveRequest";

    /// <summary><c>messageType</c> of the response reporting per-item move results.</summary>
    public const string ItemMoveResponse = "ItemMoveResponse";

    /// <summary><c>messageType</c> for a request sorting an item between model layers.</summary>
    public const string ItemSortRequest = "ItemSortRequest";

    /// <summary><c>messageType</c> of the response confirming the applied sorting.</summary>
    public const string ItemSortResponse = "ItemSortResponse";

    /// <summary><c>messageType</c> for a request pinning an item to the model.</summary>
    public const string ItemPinRequest = "ItemPinRequest";

    /// <summary><c>messageType</c> of the response confirming the pin state.</summary>
    public const string ItemPinResponse = "ItemPinResponse";

    // Tracking --------------------------------------------------------------

    /// <summary><c>messageType</c> for a request asking whether a face is currently being tracked.</summary>
    public const string FaceFoundRequest = "FaceFoundRequest";

    /// <summary><c>messageType</c> of the response reporting whether a face is currently found.</summary>
    public const string FaceFoundResponse = "FaceFoundResponse";

    // Events ----------------------------------------------------------------

    /// <summary><c>messageType</c> for a request subscribing to or unsubscribing from an event.</summary>
    public const string EventSubscriptionRequest = "EventSubscriptionRequest";

    /// <summary><c>messageType</c> of the response listing the events the session is now subscribed to.</summary>
    public const string EventSubscriptionResponse = "EventSubscriptionResponse";

    // Error -----------------------------------------------------------------

    /// <summary><c>messageType</c> of an error frame returned when a request fails.</summary>
    public const string ApiError = "APIError";
}
