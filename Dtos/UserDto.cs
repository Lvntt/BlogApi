using System.ComponentModel.DataAnnotations;
using BlogApi.Models.Types;

namespace BlogApi.Dtos;

public class UserDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    [MinLength(1)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public DateTime? BirthDate { get; set; }
    
    [Required]
    public Gender Gender { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [Required]
    public DateTime CreateTime { get; set; }
}