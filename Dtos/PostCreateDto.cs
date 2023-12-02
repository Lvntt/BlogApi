using System.ComponentModel.DataAnnotations;
using BlogApi.Dtos.ValidationAttributes;

namespace BlogApi.Dtos;

public class PostCreateDto
{
    [Required]
    [MinLength(5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(5)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 10000)]
    public int ReadingTime { get; set; }
    
    [Url]
    public string? Image { get; set; }
    
    [ValidAddress]
    public Guid? AddressId { get; set; }

    [ValidTags]
    public List<Guid> Tags { get; set; } = new();
}