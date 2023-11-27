using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos;

public class PostCreateDto
{
    // TODO add attributes ValidAddress, ValidTag etc.
    [Required]
    [MinLength(5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(5)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int ReadingTime { get; set; }
    
    [Url]
    public string? Image { get; set; }
    
    public Guid? AddressId { get; set; }
    
    public List<Guid>? Tags { get; set; }
}