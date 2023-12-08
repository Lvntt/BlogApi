using System.ComponentModel.DataAnnotations;
using BlogApi.Data.Repositories.AddressRepo;
using BlogApi.Services.AddressService;

namespace BlogApi.Dtos.ValidationAttributes;

public class ValidAddress : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var addressService = (IAddressService)validationContext.GetService(typeof(IAddressService))!;
        var addressId = ((PostCreateDto)validationContext.ObjectInstance).AddressId;

        if (addressId == null)
        {
            return ValidationResult.Success;
        }

        var task = addressService.Chain((Guid)addressId);
        task.GetAwaiter().GetResult();

        return ValidationResult.Success;
    }
}