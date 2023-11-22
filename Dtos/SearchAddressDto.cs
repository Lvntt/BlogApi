using BlogApi.Models.Types;

namespace BlogApi.Dtos;

public class SearchAddressDto
{
    public long ObjectId { get; set; }
    public Guid ObjectGuid { get; set; }
    public string? Text { get; set; }
    public GarAddressLevel ObjectLevel { get; set; }
    public string? ObjectLevelText { get; set; }
}