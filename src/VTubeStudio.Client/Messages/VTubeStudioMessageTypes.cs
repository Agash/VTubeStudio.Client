namespace VTubeStudio.Client.Messages;

/// <summary>Well-known <c>messageType</c> values used by the VTube Studio Public API.</summary>
public static class VTubeStudioMessageTypes
{
    // Session / state ------------------------------------------------------
    public const string ApiStateRequest = "APIStateRequest";
    public const string ApiStateResponse = "APIStateResponse";
    public const string StatisticsRequest = "StatisticsRequest";
    public const string StatisticsResponse = "StatisticsResponse";
    public const string VtsFolderInfoRequest = "VTSFolderInfoRequest";
    public const string VtsFolderInfoResponse = "VTSFolderInfoResponse";

    // Authentication --------------------------------------------------------
    public const string AuthenticationTokenRequest = "AuthenticationTokenRequest";
    public const string AuthenticationTokenResponse = "AuthenticationTokenResponse";
    public const string AuthenticationRequest = "AuthenticationRequest";
    public const string AuthenticationResponse = "AuthenticationResponse";

    // Models ----------------------------------------------------------------
    public const string CurrentModelRequest = "CurrentModelRequest";
    public const string CurrentModelResponse = "CurrentModelResponse";
    public const string AvailableModelsRequest = "AvailableModelsRequest";
    public const string AvailableModelsResponse = "AvailableModelsResponse";
    public const string ModelLoadRequest = "ModelLoadRequest";
    public const string ModelLoadResponse = "ModelLoadResponse";
    public const string MoveModelRequest = "MoveModelRequest";
    public const string MoveModelResponse = "MoveModelResponse";

    // Hotkeys & expressions ------------------------------------------------
    public const string HotkeysInCurrentModelRequest = "HotkeysInCurrentModelRequest";
    public const string HotkeysInCurrentModelResponse = "HotkeysInCurrentModelResponse";
    public const string HotkeyTriggerRequest = "HotkeyTriggerRequest";
    public const string HotkeyTriggerResponse = "HotkeyTriggerResponse";
    public const string ExpressionStateRequest = "ExpressionStateRequest";
    public const string ExpressionStateResponse = "ExpressionStateResponse";
    public const string ExpressionActivationRequest = "ExpressionActivationRequest";
    public const string ExpressionActivationResponse = "ExpressionActivationResponse";

    // ArtMesh & tints -------------------------------------------------------
    public const string ArtMeshListRequest = "ArtMeshListRequest";
    public const string ArtMeshListResponse = "ArtMeshListResponse";
    public const string ColorTintRequest = "ColorTintRequest";
    public const string ColorTintResponse = "ColorTintResponse";

    // Parameters ------------------------------------------------------------
    public const string InputParameterListRequest = "InputParameterListRequest";
    public const string InputParameterListResponse = "InputParameterListResponse";
    public const string Live2DParameterListRequest = "Live2DParameterListRequest";
    public const string Live2DParameterListResponse = "Live2DParameterListResponse";
    public const string ParameterValueRequest = "ParameterValueRequest";
    public const string ParameterValueResponse = "ParameterValueResponse";
    public const string InjectParameterDataRequest = "InjectParameterDataRequest";
    public const string InjectParameterDataResponse = "InjectParameterDataResponse";

    // Items -----------------------------------------------------------------
    public const string ItemListRequest = "ItemListRequest";
    public const string ItemListResponse = "ItemListResponse";
    public const string ItemLoadRequest = "ItemLoadRequest";
    public const string ItemLoadResponse = "ItemLoadResponse";
    public const string ItemUnloadRequest = "ItemUnloadRequest";
    public const string ItemUnloadResponse = "ItemUnloadResponse";
    public const string ItemAnimationControlRequest = "ItemAnimationControlRequest";
    public const string ItemAnimationControlResponse = "ItemAnimationControlResponse";
    public const string ItemMoveRequest = "ItemMoveRequest";
    public const string ItemMoveResponse = "ItemMoveResponse";

    // Tracking --------------------------------------------------------------
    public const string FaceFoundRequest = "FaceFoundRequest";
    public const string FaceFoundResponse = "FaceFoundResponse";

    // Events ----------------------------------------------------------------
    public const string EventSubscriptionRequest = "EventSubscriptionRequest";
    public const string EventSubscriptionResponse = "EventSubscriptionResponse";

    // Error -----------------------------------------------------------------
    public const string ApiError = "APIError";
}
