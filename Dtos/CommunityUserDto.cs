using System.ComponentModel.DataAnnotations;
using BlogApi.Models.Types;

namespace BlogApi.Dtos;

public class CommunityUserDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid CommunityId { get; set; }
    
    [Required]
    public CommunityRole Role { get; set; }
}