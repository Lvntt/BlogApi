using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos;

public class UpdateCommentDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}