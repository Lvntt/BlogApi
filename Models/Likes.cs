namespace BlogApi.Models;

public class Likes
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
}