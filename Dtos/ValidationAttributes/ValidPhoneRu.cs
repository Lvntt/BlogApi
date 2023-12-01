using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Dtos.ValidationAttributes;

public partial class ValidPhoneRu : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var phoneNumber = ((UserRegisterDto)validationContext.ObjectInstance).PhoneNumber;
        
        var regex = PhoneRuRegex();
        return phoneNumber.IsNullOrEmpty() || !regex.IsMatch(phoneNumber)
            ? new ValidationResult($"The phone number '{value}' is not valid.")
            : ValidationResult.Success;
    }

    [GeneratedRegex(@"^\+7\s\([0-9]{3}\)\s[0-9]{3}\-[0-9]{2}\-[0-9]{2}$")]
    private static partial Regex PhoneRuRegex();
}