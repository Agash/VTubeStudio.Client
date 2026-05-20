namespace VTubeStudio.Client.Errors;

/// <summary>
/// Subset of VTube Studio <c>errorID</c> values the client recognises and surfaces. The full
/// enumeration is maintained by DenchiSoft; new ids surface as <see cref="Unknown"/> with the
/// numeric value preserved on the raised exception.
/// </summary>
public enum VTubeStudioErrorId
{
    Unknown = 0,
    InternalServerError = 1,
    ApiAccessDeactivated = 2,
    JsonInvalid = 3,
    ApiNameInvalid = 4,
    ApiVersionInvalid = 5,
    RequestIdInvalid = 6,
    RequestTypeMissing = 7,
    RequestTypeUnknown = 8,
    RequestRequiresAuthentication = 50,
    RequestRequiresPermission = 51,
    TokenRequestDeniedByUser = 100,
    TokenRequestCurrentlyOngoing = 101,
    TokenRequestPluginNameInvalid = 102,
    TokenRequestDeveloperNameInvalid = 103,
    TokenRequestPluginIconInvalid = 104,
    AuthenticationTokenMissing = 150,
    AuthenticationTokenInvalid = 151,
    AuthenticationPluginNameMissing = 152,
    AuthenticationDeveloperNameMissing = 153,
    ModelIdMissing = 200,
    ModelIdInvalid = 201,
    ModelIdNotFound = 202,
    ModelLoadCooldownNotOver = 203,
    CannotCurrentlyChangeModel = 204,
    HotkeyQueueFull = 250,
    HotkeyExecutionFailedBecauseNoModelLoaded = 251,
    HotkeyExecutionFailedBecauseHotkeyNotFound = 252,
    HotkeyExecutionFailedBecauseBadState = 253,
    HotkeyExecutionFailedBecauseUnknownHotkeyType = 254,
    HotkeyExecutionFailedBecauseLive2DItemNotFound = 255,
    HotkeyExecutionFailedBecauseLive2DItemsNotAllowedInApi = 256,
    ExpressionStateRequestInvalidFilename = 350,
    ExpressionStateRequestFileNotFound = 351,
    ExpressionActivationRequestInvalidFilename = 352,
    ExpressionActivationRequestFileNotFound = 353,
    ExpressionActivationRequestNoModelLoaded = 354,
}
