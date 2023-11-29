using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos;

public class CommunityCreateDto
{
    [Required]
    [MinLength(5)]
    public string Name { get; set; }
    
    [MinLength(5)]
    public string? Description { get; set; }
    
    [Required]
    public bool IsClosed { get; set; }
}