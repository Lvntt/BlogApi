using System.ComponentModel.DataAnnotations;
using BlogApi.Data.Repositories.AddressRepo;

namespace BlogApi.Dtos.ValidationAttributes;

public class ValidAddress : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var addressRepository = (IAddressRepository)validationContext.GetService(typeof(IAddressRepository))!;
        var addressId = ((PostCreateDto)validationContext.ObjectInstance).AddressId;

        if (addressId == null)
        {
            return ValidationResult.Success;
        }

        var task = addressRepository.Chain((Guid)addressId);
        task.GetAwaiter().GetResult();

        return ValidationResult.Success;
    }
}