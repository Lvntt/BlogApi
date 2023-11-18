using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.ValidationAttributes;

public class MinDigits : ValidationAttribute
{
    private readonly int _minDigits;

    public MinDigits(int minDigits)
    {
        _minDigits = minDigits;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = ((UserRegisterDto)validationContext.ObjectInstance).Password;

        return password.Count(char.IsDigit) < _minDigits
            ? new ValidationResult($"Password must contain at least {_minDigits} digit(s).") 
            : ValidationResult.Success;
    }
}