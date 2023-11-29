using BlogApi.Models.Types;

namespace BlogApi.Models;

public class CommunityMember
{
    public Guid CommunityId { get; set; }
    public Guid UserId { get; set; }
    public CommunityRole Role { get; set; }
}