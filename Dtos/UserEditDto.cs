using System.ComponentModel.DataAnnotations;
using BlogApi.Models.Types;

namespace BlogApi.Dtos;

public class UserEditDto
{
    [MinLength(1)]
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    [MinLength(1)]
    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;
    
    public DateTime? BirthDate { get; set; }
    
    [Required]
    public Gender Gender { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
}