using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos;

public class LoginCredentialsDto
{
    [Required]
    [EmailAddress]
    [MinLength(1)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1)]
    public string Password { get; set; } = string.Empty;
}