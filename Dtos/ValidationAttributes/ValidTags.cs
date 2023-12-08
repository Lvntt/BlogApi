using System.ComponentModel.DataAnnotations;
using BlogApi.Data.Repositories.TagRepo;
using BlogApi.Services.TagService;

namespace BlogApi.Dtos.ValidationAttributes;

public class ValidTags : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var tagService = (ITagService)validationContext.GetService(typeof(ITagService))!;
        var tagGuids = ((PostCreateDto)validationContext.ObjectInstance).Tags;
        
        // foreach (var tagGuid in tagGuids)
        // {
            // TODO extension GetTagFromGuid(tagGuid)?
            // var task = tagRepository.GetTagFromGuid(tagGuid);
            // var tag = task.GetAwaiter().GetResult();
            //
            // if (tag == null)
            // {
            //     return new ValidationResult("One or more tags is not valid");
            // }
        // }
        
        return ValidationResult.Success;
    }
}