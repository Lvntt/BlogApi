using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos;

public class TagDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreateTime { get; set; }
}