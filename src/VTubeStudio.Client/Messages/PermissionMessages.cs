using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of a <c>PermissionRequest</c>: requests a permission or lists granted permissions.</summary>
public sealed record PermissionRequest
{
    /// <summary>The permission to request; empty lists granted permissions without prompting.</summary>
    [JsonPropertyName("requestedPermission")] public string? RequestedPermission { get; init; }
}

/// <summary>Payload of a <c>PermissionResponse</c>: the grant result and the permission list.</summary>
public sealed record PermissionResponse
{
    /// <summary>True when the requested permission was granted.</summary>
    [JsonPropertyName("grantSuccess")] public bool GrantSuccess { get; init; }

    /// <summary>The requested permission name.</summary>
    [JsonPropertyName("requestedPermission")] public string? RequestedPermission { get; init; }

    /// <summary>All permissions VTube Studio offers with their grant state.</summary>
    [JsonPropertyName("permissions")] public IReadOnlyList<PermissionInfo> Permissions { get; init; } = [];
}

/// <summary>One entry in <see cref="PermissionResponse.Permissions"/>.</summary>
public sealed record PermissionInfo
{
    /// <summary>The permission name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>True when this plugin currently holds the permission.</summary>
    [JsonPropertyName("granted")] public bool Granted { get; init; }
}
