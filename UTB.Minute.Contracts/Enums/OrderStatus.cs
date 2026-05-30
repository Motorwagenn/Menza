

using System.Text.Json.Serialization;

namespace UTB.Minute.Contracts.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum OrderStatus
    {
        Preparing,
        Ready,
        Cancelled,
        Completed
    }
}
