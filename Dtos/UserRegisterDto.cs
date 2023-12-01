using System.ComponentModel.DataAnnotations;
using BlogApi.Dtos.ValidationAttributes;
using BlogApi.Models.Types;

namespace BlogApi.Dtos;

public class UserRegisterDto
{
    [Required]
    [MinLength(1)] 
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MinDigits(1)]
    [MinLetters(1)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [ValidBirthDate]
    public DateTime? BirthDate { get; set; }
    
    [Required]
    public Gender Gender { get; set; }
    
    [ValidPhoneRu]
    public string? PhoneNumber { get; set; }
}