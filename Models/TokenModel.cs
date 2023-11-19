using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

public class TokenModel
{
    [Key]
    public string Token { get; set; } = string.Empty;
}