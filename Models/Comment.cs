using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

public class Comment
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public DateTime CreateTime { get; set; }
    
    public Guid? ParentCommentId { get; set; }
    public Guid? TopLevelCommentId { get; set; }
    public Guid PostId { get; set; }
    
    [Required]
    [MinLength(1)] 
    public string Content { get; set; } = string.Empty;
    
    public DateTime? ModifiedDate { get; set; }
    
    public DateTime? DeleteDate { get; set; }
    
    [Required]
    public Guid AuthorId { get; set; }

    [Required]
    [MinLength(1)]
    public string Author { get; set; } = string.Empty;

    [Required]
    public int SubComments { get; set; }
}