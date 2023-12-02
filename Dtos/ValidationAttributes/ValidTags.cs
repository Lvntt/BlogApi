using System.ComponentModel.DataAnnotations;
using BlogApi.Data.Repositories.TagRepo;

namespace BlogApi.Dtos.ValidationAttributes;

public class ValidTags : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var tagRepository = (ITagRepository)validationContext.GetService(typeof(ITagRepository))!;
        var tagGuids = ((PostCreateDto)validationContext.ObjectInstance).Tags;
        
        foreach (var tagGuid in tagGuids)
        {
            var task = tagRepository.GetTagFromGuid(tagGuid);
            var tag = task.GetAwaiter().GetResult();
            
            if (tag == null)
            {
                return new ValidationResult("One or more tags is not valid");
            }
        }
        
        return ValidationResult.Success;
    }
}