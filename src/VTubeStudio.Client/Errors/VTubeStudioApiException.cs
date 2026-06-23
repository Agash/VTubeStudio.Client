namespace VTubeStudio.Client.Errors;

/// <summary>
/// Raised when a VTube Studio request returns a typed error response (<c>messageType == "APIError"</c>).
/// The numeric <see cref="ErrorIdRaw"/> is always preserved even when <see cref="ErrorId"/> is
/// <see cref="VTubeStudioErrorId.Unknown"/>.
/// </summary>
public sealed class VTubeStudioApiException(VTubeStudioErrorId errorId, int errorIdRaw, string apiMessage)
    : Exception($"[{errorIdRaw}] {apiMessage}")
{
    /// <summary>The recognised error id, or <see cref="VTubeStudioErrorId.Unknown"/> when the raw id is not mapped.</summary>
    public VTubeStudioErrorId ErrorId { get; } = errorId;

    /// <summary>The raw numeric <c>errorID</c> exactly as sent by VTube Studio, preserved even when unrecognised.</summary>
    public int ErrorIdRaw { get; } = errorIdRaw;

    /// <summary>The human-readable error message returned by VTube Studio.</summary>
    public string ApiMessage { get; } = apiMessage;
}
