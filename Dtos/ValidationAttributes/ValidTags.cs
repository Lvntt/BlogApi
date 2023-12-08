using System.ComponentModel.DataAnnotations;
using BlogApi.Data.DbContext;
using BlogApi.Extensions;

namespace BlogApi.Dtos.ValidationAttributes;

public class ValidTags : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var blogDbContext = (BlogDbContext)validationContext.GetService(typeof(BlogDbContext))!;
        var tagGuids = ((PostCreateDto)validationContext.ObjectInstance).Tags;
        
        foreach (var tagGuid in tagGuids)
        {
            var task = blogDbContext.GetTagById(tagGuid);
            task.GetAwaiter().GetResult();
        }
        
        return ValidationResult.Success;
    }
}