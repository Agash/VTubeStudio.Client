using System.Text.Json;
using VTubeStudio.Client.Messages;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client.Tests;

[TestClass]
public sealed class ExpressionRequestTests
{
    [TestMethod]
    public void ExpressionActivationRequest_WithFadeTime_SendsIt()
    {
        string json = JsonSerializer.Serialize(
            new ExpressionActivationRequest
            {
                ExpressionFile = "smile.exp3.json",
                Active = true,
                FadeTime = 0.5,
            },
            VTubeStudioJsonContext.Default.ExpressionActivationRequest
        );

        using JsonDocument document = JsonDocument.Parse(json);
        Assert.AreEqual(0.5, document.RootElement.GetProperty("fadeTime").GetDouble());
    }

    [TestMethod]
    public void ExpressionActivationRequest_WithoutFadeTime_LeavesVTubeStudiosDefault()
    {
        string json = JsonSerializer.Serialize(
            new ExpressionActivationRequest { ExpressionFile = "smile.exp3.json", Active = true },
            VTubeStudioJsonContext.Default.ExpressionActivationRequest
        );

        using JsonDocument document = JsonDocument.Parse(json);
        Assert.IsFalse(document.RootElement.TryGetProperty("fadeTime", out _));
    }
}
