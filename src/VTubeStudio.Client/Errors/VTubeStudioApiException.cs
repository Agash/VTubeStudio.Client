namespace VTubeStudio.Client.Errors;

/// <summary>
/// Raised when a VTube Studio request returns a typed error response (<c>messageType == "APIError"</c>).
/// The numeric <see cref="ErrorIdRaw"/> is always preserved even when <see cref="ErrorId"/> is
/// <see cref="VTubeStudioErrorId.Unknown"/>.
/// </summary>
public sealed class VTubeStudioApiException(VTubeStudioErrorId errorId, int errorIdRaw, string apiMessage)
    : Exception($"[{errorIdRaw}] {apiMessage}")
{
    public VTubeStudioErrorId ErrorId { get; } = errorId;

    public int ErrorIdRaw { get; } = errorIdRaw;

    public string ApiMessage { get; } = apiMessage;
}
