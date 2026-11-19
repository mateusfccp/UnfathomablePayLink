namespace UnfathomablePayLink.Routing;

/// <summary>
/// Transforms route tokens (such as <c>[controller]</c> and <c>[action]</c>) to lowercase.
/// </summary>
public sealed class LowercaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value) =>
        value?.ToString()?.ToLowerInvariant();
}
