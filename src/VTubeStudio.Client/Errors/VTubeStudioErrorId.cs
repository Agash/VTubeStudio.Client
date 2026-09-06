namespace VTubeStudio.Client.Errors;

/// <summary>
/// VTube Studio <c>errorID</c> values. Mirrors the official <c>ErrorID</c>
/// enumeration maintained by DenchiSoft; values match the wire protocol.
/// Unrecognised ids surface as <see cref="Unknown"/> with the numeric value
/// preserved on the raised exception.
/// </summary>
public enum VTubeStudioErrorId
{
    /// <summary>Library-side sentinel, not an upstream error id. Used when the received numeric id is unrecognized; the original value is preserved on the raised exception.</summary>
    Unknown = -1,

    /// <summary>An unexpected internal error occurred inside VTube Studio (errorID 0).</summary>
    InternalServerError = 0,

    /// <summary>The API is currently switched off in the VTube Studio settings (errorID 1).</summary>
    ApiAccessDeactivated = 1,

    /// <summary>The received message was not valid JSON (errorID 2).</summary>
    JsonInvalid = 2,

    /// <summary>The <c>apiName</c> field was missing or not <c>"VTubeStudioPublicAPI"</c> (errorID 3).</summary>
    ApiNameInvalid = 3,

    /// <summary>The <c>apiVersion</c> field was missing or unsupported (errorID 4).</summary>
    ApiVersionInvalid = 4,

    /// <summary>The <c>requestID</c> field was invalid (too long or otherwise malformed) (errorID 5).</summary>
    RequestIdInvalid = 5,

    /// <summary>The request was missing its <c>messageType</c> (errorID 6).</summary>
    RequestTypeMissingOrEmpty = 6,

    /// <summary>The request <c>messageType</c> was not a recognised request type (errorID 7).</summary>
    RequestTypeUnknown = 7,

    /// <summary>The request requires the session to be authenticated first (errorID 8).</summary>
    RequestRequiresAuthentication = 8,

    /// <summary>The request requires a permission the user has not granted (errorID 9).</summary>
    RequestRequiresPermission = 9,

    /// <summary>The user denied the authentication-token request in the VTube Studio UI (errorID 50).</summary>
    TokenRequestDenied = 50,

    /// <summary>An authentication-token request is already in progress for this plugin (errorID 51).</summary>
    TokenRequestCurrentlyOngoing = 51,

    /// <summary>The supplied plugin name was missing or outside the allowed length (errorID 52).</summary>
    TokenRequestPluginNameInvalid = 52,

    /// <summary>The supplied developer name was missing or outside the allowed length (errorID 53).</summary>
    TokenRequestDeveloperNameInvalid = 53,

    /// <summary>The supplied plugin icon was not a valid base64-encoded 128×128 image (errorID 54).</summary>
    TokenRequestPluginIconInvalid = 54,

    /// <summary>The authentication request was missing the <c>authenticationToken</c> field (errorID 100).</summary>
    AuthenticationTokenMissing = 100,

    /// <summary>The authentication request was missing the plugin name (errorID 101).</summary>
    AuthenticationPluginNameMissing = 101,

    /// <summary>The authentication request was missing the developer name (errorID 102).</summary>
    AuthenticationPluginDeveloperMissing = 102,

    /// <summary>A model request was missing the required <c>modelID</c> (errorID 150).</summary>
    ModelIdMissing = 150,

    /// <summary>The supplied <c>modelID</c> was malformed (errorID 151).</summary>
    ModelIdInvalid = 151,

    /// <summary>No model with the supplied <c>modelID</c> exists on the machine (errorID 152).</summary>
    ModelIdNotFound = 152,

    /// <summary>A model was loaded too recently; the model-load cooldown has not elapsed (errorID 153).</summary>
    ModelLoadCooldownNotOver = 153,

    /// <summary>The model cannot currently be changed (for example, another change is in progress) (errorID 154).</summary>
    CannotCurrentlyChangeModel = 154,

    /// <summary>The hotkey trigger queue is full; the request was dropped (errorID 200).</summary>
    HotkeyQueueFull = 200,

    /// <summary>The hotkey could not be triggered because no model is loaded (errorID 201).</summary>
    HotkeyExecutionFailedBecauseNoModelLoaded = 201,

    /// <summary>The hotkey id was not found in the model (errorID 202).</summary>
    HotkeyIdNotFoundInModel = 202,

    /// <summary>The hotkey could not be triggered because its cooldown has not elapsed (errorID 203).</summary>
    HotkeyCooldownNotOver = 203,

    /// <summary>The hotkey was found but its data is invalid, for example missing files (errorID 204).</summary>
    HotkeyIdFoundButHotkeyDataInvalid = 204,

    /// <summary>The hotkey could not be triggered because VTube Studio was in a bad state (errorID 205).</summary>
    HotkeyExecutionFailedBecauseBadState = 205,

    /// <summary>The hotkey trigger failed for an unknown reason (errorID 206).</summary>
    HotkeyUnknownExecutionFailure = 206,

    /// <summary>The hotkey targeted a Live2D item instance that was not found (errorID 207).</summary>
    HotkeyExecutionFailedBecauseLive2DItemNotFound = 207,

    /// <summary>The hotkey type is not supported on Live2D items (errorID 208).</summary>
    HotkeyExecutionFailedBecauseLive2DItemsDoNotSupportThisHotkeyType = 208,

    /// <summary>The color-tint request arrived while no model is loaded (errorID 250).</summary>
    ColorTintRequestNoModelLoaded = 250,

    /// <summary>The color-tint request supplied neither a matcher nor a color (errorID 251).</summary>
    ColorTintRequestMatchOrColorMissing = 251,

    /// <summary>The color-tint request supplied an invalid color value (errorID 252).</summary>
    ColorTintRequestInvalidColorValue = 252,

    /// <summary>The move-model request arrived while no model is loaded (errorID 300).</summary>
    MoveModelRequestNoModelLoaded = 300,

    /// <summary>The move-model request is missing required fields (errorID 301).</summary>
    MoveModelRequestMissingFields = 301,

    /// <summary>The move-model request supplied out-of-range values (errorID 302).</summary>
    MoveModelRequestValuesOutOfRange = 302,

    /// <summary>The custom parameter name is invalid (errorID 350).</summary>
    CustomParamNameInvalid = 350,

    /// <summary>The custom parameter values are invalid (errorID 351).</summary>
    CustomParamValuesInvalid = 351,

    /// <summary>The parameter name is already used by a different plugin (errorID 352).</summary>
    CustomParamAlreadyCreatedByOtherPlugin = 352,

    /// <summary>The custom parameter explanation exceeds 256 characters (errorID 353).</summary>
    CustomParamExplanationTooLong = 353,

    /// <summary>The name collides with a default parameter name (errorID 354).</summary>
    CustomParamDefaultParamNameNotAllowed = 354,

    /// <summary>The per-plugin custom parameter limit is exceeded (errorID 355).</summary>
    CustomParamLimitPerPluginExceeded = 355,

    /// <summary>The global custom parameter limit is exceeded (errorID 356).</summary>
    CustomParamLimitTotalExceeded = 356,

    /// <summary>The deleted parameter name is invalid (errorID 400).</summary>
    CustomParamDeletionNameInvalid = 400,

    /// <summary>The parameter to delete was not found (errorID 401).</summary>
    CustomParamDeletionNotFound = 401,

    /// <summary>The parameter was created by a different plugin (errorID 402).</summary>
    CustomParamDeletionCreatedByOtherPlugin = 402,

    /// <summary>Default parameters cannot be deleted (errorID 403).</summary>
    CustomParamDeletionCannotDeleteDefaultParam = 403,

    /// <summary>No parameter data was provided for injection (errorID 450).</summary>
    InjectDataNoDataProvided = 450,

    /// <summary>An injected value is invalid (errorID 451).</summary>
    InjectDataValueInvalid = 451,

    /// <summary>An injected weight is invalid (errorID 452).</summary>
    InjectDataWeightInvalid = 452,

    /// <summary>Data was sent for a parameter that does not exist (errorID 453).</summary>
    InjectDataParamNameNotFound = 453,

    /// <summary>The parameter is already controlled by a different plugin (errorID 454).</summary>
    InjectDataParamControlledByOtherPlugin = 454,

    /// <summary>The inject mode is not <c>"set"</c> or <c>"add"</c> (errorID 455).</summary>
    InjectDataModeUnknown = 455,

    /// <summary>The queried parameter was not found (errorID 500).</summary>
    ParameterValueRequestParameterNotFound = 500,

    /// <summary>The NDI config cooldown has not elapsed (errorID 550).</summary>
    NdiConfigCooldownNotOver = 550,

    /// <summary>The NDI resolution is invalid (errorID 551).</summary>
    NdiConfigResolutionInvalid = 551,

    /// <summary>The expression-state request supplied an invalid expression file name (errorID 600).</summary>
    ExpressionStateRequestInvalidFilename = 600,

    /// <summary>The expression file referenced in the state request was not found (errorID 601).</summary>
    ExpressionStateRequestFileNotFound = 601,

    /// <summary>The expression-activation request supplied an invalid expression file name (errorID 650).</summary>
    ExpressionActivationRequestInvalidFilename = 650,

    /// <summary>The expression file referenced in the activation request was not found (errorID 651).</summary>
    ExpressionActivationRequestFileNotFound = 651,

    /// <summary>An expression cannot be activated because no model is loaded (errorID 652).</summary>
    ExpressionActivationRequestNoModelLoaded = 652,

    /// <summary>The physics request arrived while no model is loaded (errorID 700).</summary>
    SetCurrentModelPhysicsRequestNoModelLoaded = 700,

    /// <summary>The model has no physics setup (errorID 701).</summary>
    SetCurrentModelPhysicsRequestModelHasNoPhysics = 701,

    /// <summary>Physics is currently controlled by a different plugin (errorID 702).</summary>
    SetCurrentModelPhysicsRequestPhysicsControlledByOtherPlugin = 702,

    /// <summary>No overrides were provided (errorID 703).</summary>
    SetCurrentModelPhysicsRequestNoOverridesProvided = 703,

    /// <summary>A physics group id was not found (errorID 704).</summary>
    SetCurrentModelPhysicsRequestPhysicsGroupIdNotFound = 704,

    /// <summary>An override has no value (errorID 705).</summary>
    SetCurrentModelPhysicsRequestNoOverrideValueProvided = 705,

    /// <summary>A physics group id was provided twice (errorID 706).</summary>
    SetCurrentModelPhysicsRequestDuplicatePhysicsGroupId = 706,

    /// <summary>The item file name is missing (errorID 750).</summary>
    ItemFileNameMissing = 750,

    /// <summary>The item file was not found (errorID 751).</summary>
    ItemFileNameNotFound = 751,

    /// <summary>Unused upstream; the item-load cooldown was removed (errorID 752).</summary>
    ItemLoadLoadCooldownNotOver = 752,

    /// <summary>The item cannot currently be loaded, usually because menus are open (errorID 753).</summary>
    CannotCurrentlyLoadItem = 753,

    /// <summary>The scene holds the maximum number of items (errorID 754).</summary>
    CannotLoadItemSceneFull = 754,

    /// <summary>The requested item order is invalid (errorID 755).</summary>
    ItemOrderInvalid = 755,

    /// <summary>The requested item order is already taken (errorID 756).</summary>
    ItemOrderAlreadyTaken = 756,

    /// <summary>Item load values are invalid (errorID 757).</summary>
    ItemLoadValuesInvalid = 757,

    /// <summary>The custom image data is invalid (errorID 758).</summary>
    ItemCustomDataInvalid = 758,

    /// <summary>The maximum number of custom-image prompts is already shown (errorID 759).</summary>
    ItemCustomDataCannotAskRightNow = 759,

    /// <summary>The user rejected the custom-image load (errorID 760).</summary>
    ItemCustomDataLoadRequestRejectedByUser = 760,

    /// <summary>The item cannot currently be unloaded, usually because menus are open (errorID 800).</summary>
    CannotCurrentlyUnloadItem = 800,

    /// <summary>The item instance id was not found (errorID 850).</summary>
    ItemAnimationControlInstanceIdNotFound = 850,

    /// <summary>The item type does not support animation control, for example Live2D items (errorID 851).</summary>
    ItemAnimationControlUnsupportedItemType = 851,

    /// <summary>Auto-stop frame indices are invalid (errorID 852).</summary>
    ItemAnimationControlAutoStopFramesInvalid = 852,

    /// <summary>Too many auto-stop frames; maximum is 1024 (errorID 853).</summary>
    ItemAnimationControlTooManyAutoStopFrames = 853,

    /// <summary>Static images do not support animation controls (errorID 854).</summary>
    ItemAnimationControlSimpleImageDoesNotSupportAnim = 854,

    /// <summary>The item instance id was not found (errorID 900).</summary>
    ItemMoveRequestInstanceIdNotFound = 900,

    /// <summary>The fade mode is invalid (errorID 901).</summary>
    ItemMoveRequestInvalidFadeMode = 901,

    /// <summary>The item order is taken or invalid (errorID 902).</summary>
    ItemMoveRequestItemOrderTakenOrInvalid = 902,

    /// <summary>The item order cannot currently be changed because windows are open (errorID 903).</summary>
    ItemMoveRequestCannotCurrentlyChangeOrder = 903,

    /// <summary>The event type is unknown (errorID 950).</summary>
    EventSubscriptionRequestEventTypeUnknown = 950,

    /// <summary>No model is loaded for the ArtMesh selection request (errorID 1000).</summary>
    ArtMeshSelectionRequestNoModelLoaded = 1000,

    /// <summary>Other windows are open; the selection cannot start (errorID 1001).</summary>
    ArtMeshSelectionRequestOtherWindowsOpen = 1001,

    /// <summary>A pre-activated ArtMesh does not exist in the model (errorID 1002).</summary>
    ArtMeshSelectionRequestModelDoesNotHaveArtMesh = 1002,

    /// <summary>The pre-activated ArtMesh list is too long (errorID 1003).</summary>
    ArtMeshSelectionRequestArtMeshIdListError = 1003,

    /// <summary>The item to pin is not loaded (errorID 1050).</summary>
    ItemPinRequestGivenItemNotLoaded = 1050,

    /// <summary>The pin angle or size type is invalid (errorID 1051).</summary>
    ItemPinRequestInvalidAngleOrSizeType = 1051,

    /// <summary>The pin model was not found (errorID 1052).</summary>
    ItemPinRequestModelNotFound = 1052,

    /// <summary>The pin ArtMesh was not found (errorID 1053).</summary>
    ItemPinRequestArtMeshNotFound = 1053,

    /// <summary>The pin position is invalid (errorID 1054).</summary>
    ItemPinRequestPinPositionInvalid = 1054,

    /// <summary>The requested permission is unknown (errorID 1100).</summary>
    PermissionRequestUnknownPermission = 1100,

    /// <summary>The permission cannot currently be requested, for example the config window is open (errorID 1101).</summary>
    PermissionRequestCannotRequestRightNow = 1101,

    /// <summary>The plugin permission file could not be read or written (errorID 1102).</summary>
    PermissionRequestFileProblem = 1102,

    /// <summary>The post-processing filter array exceeds 512 entries (errorID 1150).</summary>
    PostProcessingListRequestInvalidFilter = 1150,

    /// <summary>Post-processing cannot currently be updated because windows are open (errorID 1200).</summary>
    PostProcessingUpdateRequestCannotUpdateRightNow = 1200,

    /// <summary>The post-processing fade time is invalid (errorID 1201).</summary>
    PostProcessingUpdateRequestFadeTimeInvalid = 1201,

    /// <summary>A preset and individual values cannot be set in one request (errorID 1202).</summary>
    PostProcessingUpdateRequestLoadingPresetAndValues = 1202,

    /// <summary>The post-processing preset file was not found (errorID 1203).</summary>
    PostProcessingUpdateRequestPresetFileLoadFailed = 1203,

    /// <summary>A post-processing value list entry is invalid (errorID 1204).</summary>
    PostProcessingUpdateRequestValueListInvalid = 1204,

    /// <summary>The post-processing value list contains duplicates (errorID 1205).</summary>
    PostProcessingUpdateRequestValueListContainsDuplicates = 1205,

    /// <summary>A restricted effect was used without allowance (errorID 1206).</summary>
    PostProcessingUpdateRequestTriedToLoadRestrictedEffect = 1206,

    /// <summary>The item instance id was not found (errorID 1250).</summary>
    ItemSortRequestInstanceIdNotFound = 1250,

    /// <summary>A value-set type is invalid (errorID 1251).</summary>
    ItemSortRequestInvalidValueSetType = 1251,

    /// <summary>The front order is invalid (errorID 1252).</summary>
    ItemSortRequestInvalidFrontOrder = 1252,

    /// <summary>The back order is invalid (errorID 1253).</summary>
    ItemSortRequestInvalidBackOrder = 1253,

    /// <summary>The split point is invalid (errorID 1254).</summary>
    ItemSortRequestInvalidSplitPoint = 1254,

    /// <summary>The within-model sorting window is currently open (errorID 1255).</summary>
    ItemSortRequestItemConfigWindowOpen = 1255,

    /// <summary>The test event message exceeds 32 characters (errorID 100000).</summary>
    EventTestEventTestMessageTooLong = 100000,

    /// <summary>A model-loaded event filter id is invalid (errorID 100050).</summary>
    EventModelLoadedEventModelIdInvalid = 100050,

    /// <summary>A hotkey-triggered event action filter is invalid (errorID 100100).</summary>
    EventHotkeyTriggeredEventHotkeyActionInvalid = 100100,

    /// <summary>ArtMesh tracking points are invalid (errorID 100150).</summary>
    EventArtMeshTrackingEventTrackingPointsInvalid = 100150,

    /// <summary>The ArtMesh tracking frequency is invalid (errorID 100151).</summary>
    EventArtMeshTrackingEventFrequencyInvalid = 100151,

    /// <summary>ArtMesh outline entries are invalid (errorID 100200).</summary>
    EventArtMeshOutlineEventArtMeshesInvalid = 100200,

    /// <summary>The ArtMesh outline frequency is invalid (errorID 100201).</summary>
    EventArtMeshOutlineEventFrequencyInvalid = 100201,
}
