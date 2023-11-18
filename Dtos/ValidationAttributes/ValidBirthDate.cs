using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.ValidationAttributes;

public class ValidBirthDate : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var birthDate = ((UserRegisterDto)validationContext.ObjectInstance).BirthDate;

        return birthDate != null && birthDate > DateTime.Today
            ? new ValidationResult("Birth date can't be later than today")
            : ValidationResult.Success;
    }
}