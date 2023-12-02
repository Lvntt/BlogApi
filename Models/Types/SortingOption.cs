using System.Text.Json.Serialization;

namespace BlogApi.Models.Types;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortingOption
{
    CreateDesc,
    CreateAsc,
    LikeDesc,
    LikeAsc
}