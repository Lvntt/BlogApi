using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

public class Tag
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreateTime { get; set; }
    
    public List<Post>? Posts { get; set; }
}