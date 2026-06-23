namespace VTubeStudio.Client.Errors;

/// <summary>
/// Subset of VTube Studio <c>errorID</c> values the client recognises and surfaces. The full
/// enumeration is maintained by DenchiSoft; new ids surface as <see cref="Unknown"/> with the
/// numeric value preserved on the raised exception.
/// </summary>
public enum VTubeStudioErrorId
{
    /// <summary>An error id that the client does not recognise. The raw numeric id is preserved on the exception.</summary>
    Unknown = 0,

    /// <summary>An unexpected internal error occurred inside VTube Studio (errorID 1).</summary>
    InternalServerError = 1,

    /// <summary>The API is currently switched off in the VTube Studio settings (errorID 2).</summary>
    ApiAccessDeactivated = 2,

    /// <summary>The received message was not valid JSON (errorID 3).</summary>
    JsonInvalid = 3,

    /// <summary>The <c>apiName</c> field was missing or not <c>"VTubeStudioPublicAPI"</c> (errorID 4).</summary>
    ApiNameInvalid = 4,

    /// <summary>The <c>apiVersion</c> field was missing or unsupported (errorID 5).</summary>
    ApiVersionInvalid = 5,

    /// <summary>The <c>requestID</c> field was invalid (too long or otherwise malformed) (errorID 6).</summary>
    RequestIdInvalid = 6,

    /// <summary>The request was missing its <c>messageType</c> (errorID 7).</summary>
    RequestTypeMissing = 7,

    /// <summary>The request <c>messageType</c> was not a recognised request type (errorID 8).</summary>
    RequestTypeUnknown = 8,

    /// <summary>The request requires the session to be authenticated first (errorID 50).</summary>
    RequestRequiresAuthentication = 50,

    /// <summary>The request requires a permission the user has not granted (errorID 51).</summary>
    RequestRequiresPermission = 51,

    /// <summary>The user denied the authentication-token request in the VTube Studio UI (errorID 100).</summary>
    TokenRequestDeniedByUser = 100,

    /// <summary>An authentication-token request is already in progress for this plugin (errorID 101).</summary>
    TokenRequestCurrentlyOngoing = 101,

    /// <summary>The supplied plugin name was missing or outside the allowed length (errorID 102).</summary>
    TokenRequestPluginNameInvalid = 102,

    /// <summary>The supplied developer name was missing or outside the allowed length (errorID 103).</summary>
    TokenRequestDeveloperNameInvalid = 103,

    /// <summary>The supplied plugin icon was not a valid base64-encoded 128×128 image (errorID 104).</summary>
    TokenRequestPluginIconInvalid = 104,

    /// <summary>The authentication request was missing the <c>authenticationToken</c> field (errorID 150).</summary>
    AuthenticationTokenMissing = 150,

    /// <summary>The supplied authentication token was not valid (errorID 151).</summary>
    AuthenticationTokenInvalid = 151,

    /// <summary>The authentication request was missing the plugin name (errorID 152).</summary>
    AuthenticationPluginNameMissing = 152,

    /// <summary>The authentication request was missing the developer name (errorID 153).</summary>
    AuthenticationDeveloperNameMissing = 153,

    /// <summary>A model request was missing the required <c>modelID</c> (errorID 200).</summary>
    ModelIdMissing = 200,

    /// <summary>The supplied <c>modelID</c> was malformed (errorID 201).</summary>
    ModelIdInvalid = 201,

    /// <summary>No model with the supplied <c>modelID</c> exists on the machine (errorID 202).</summary>
    ModelIdNotFound = 202,

    /// <summary>A model was loaded too recently; the model-load cooldown has not elapsed (errorID 203).</summary>
    ModelLoadCooldownNotOver = 203,

    /// <summary>The model cannot currently be changed (for example, another change is in progress) (errorID 204).</summary>
    CannotCurrentlyChangeModel = 204,

    /// <summary>The hotkey trigger queue is full; the request was dropped (errorID 250).</summary>
    HotkeyQueueFull = 250,

    /// <summary>The hotkey could not be triggered because no model is loaded (errorID 251).</summary>
    HotkeyExecutionFailedBecauseNoModelLoaded = 251,

    /// <summary>The hotkey could not be triggered because no matching hotkey was found (errorID 252).</summary>
    HotkeyExecutionFailedBecauseHotkeyNotFound = 252,

    /// <summary>The hotkey could not be triggered because VTube Studio was in a bad state (errorID 253).</summary>
    HotkeyExecutionFailedBecauseBadState = 253,

    /// <summary>The hotkey could not be triggered because its action type is not recognised (errorID 254).</summary>
    HotkeyExecutionFailedBecauseUnknownHotkeyType = 254,

    /// <summary>The hotkey targeted a Live2D item instance that was not found (errorID 255).</summary>
    HotkeyExecutionFailedBecauseLive2DItemNotFound = 255,

    /// <summary>Triggering hotkeys on Live2D items is not currently allowed via the API (errorID 256).</summary>
    HotkeyExecutionFailedBecauseLive2DItemsNotAllowedInApi = 256,

    /// <summary>The expression-state request supplied an invalid expression file name (errorID 350).</summary>
    ExpressionStateRequestInvalidFilename = 350,

    /// <summary>The expression file referenced in the state request was not found (errorID 351).</summary>
    ExpressionStateRequestFileNotFound = 351,

    /// <summary>The expression-activation request supplied an invalid expression file name (errorID 352).</summary>
    ExpressionActivationRequestInvalidFilename = 352,

    /// <summary>The expression file referenced in the activation request was not found (errorID 353).</summary>
    ExpressionActivationRequestFileNotFound = 353,

    /// <summary>An expression cannot be activated because no model is loaded (errorID 354).</summary>
    ExpressionActivationRequestNoModelLoaded = 354,
}
