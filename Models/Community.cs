using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

public class Community
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public DateTime CreateTime { get; set; }

    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public bool IsClosed { get; set; }
    
    [Required]
    public int SubscribersCount { get; set; }
    
    [Required]
    public List<CommunityMember> Members { get; set; } = new();
}