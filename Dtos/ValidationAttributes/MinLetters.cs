using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.ValidationAttributes;

public class MinLetters : ValidationAttribute
{
    private readonly int _minLetters;

    public MinLetters(int minLetters)
    {
        _minLetters = minLetters;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = ((UserRegisterDto)validationContext.ObjectInstance).Password;

        return password.Count(char.IsLetter) < _minLetters
            ? new ValidationResult($"Password must contain at least {_minLetters} letter(s).") 
            : ValidationResult.Success;
    }
}