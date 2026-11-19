using System.Text.Json.Serialization;

namespace UnfathomablePayLink.Enums;

/// <summary>
/// Enum <c>PaymentState</c> represents one of the possible payment states.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentState
{
    [JsonStringEnumMemberName("APPROVED")]
    Approved,

    [JsonStringEnumMemberName("REJECTED")]
    Rejected,

    [JsonStringEnumMemberName("PENDING")]
    Pending
}
