using System.Text.Json.Serialization;

namespace UnfathomablePayLink.Enums;

/// <summary>
/// Represents the possible processing states of a payment transaction.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentState
{
    /// <summary>
    /// The payment was processed and approved.
    /// </summary>
    [JsonStringEnumMemberName("APPROVED")]
    Approved,

    /// <summary>
    /// The payment was processed and rejected.
    /// </summary>
    [JsonStringEnumMemberName("REJECTED")]
    Rejected,

    /// <summary>
    /// The payment has been created and is awaiting asynchronous processing.
    /// </summary>
    [JsonStringEnumMemberName("PENDING")]
    Pending
}
