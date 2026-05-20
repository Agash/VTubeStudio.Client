namespace VTubeStudio.Client.Errors;

/// <summary>
/// Raised when a VTube Studio request returns a typed error response (<c>messageType == "APIError"</c>).
/// The numeric <see cref="ErrorIdRaw"/> is always preserved even when <see cref="ErrorId"/> is
/// <see cref="VTubeStudioErrorId.Unknown"/>.
/// </summary>
public sealed class VTubeStudioApiException : Exception
{
    public VTubeStudioApiException(VTubeStudioErrorId errorId, int errorIdRaw, string apiMessage)
        : base($"[{errorIdRaw}] {apiMessage}")
    {
        ErrorId = errorId;
        ErrorIdRaw = errorIdRaw;
        ApiMessage = apiMessage;
    }

    public VTubeStudioErrorId ErrorId { get; }

    public int ErrorIdRaw { get; }

    public string ApiMessage { get; }
}
