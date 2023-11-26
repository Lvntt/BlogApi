using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos;

public class TagCreateDto
{
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
}