using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

public class Author
{
    // TODO validation: check if the user exists
    [Key]
    public Guid UserId { get; set; }
    
    public int Posts { get; set; }
    
    public int Likes { get; set; }
}