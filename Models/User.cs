using System.ComponentModel.DataAnnotations;
using BlogApi.Models.Types;

namespace BlogApi.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MinLength(1)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public DateTime? BirthDate { get; set; }
    
    [Required]
    public Gender Gender { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
}