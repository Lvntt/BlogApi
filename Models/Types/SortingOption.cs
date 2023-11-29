using System.Text.Json.Serialization;

namespace BlogApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortingOption
{
    CreateDesc,
    CreateAsc,
    LikeDesc,
    LikeAsc
}