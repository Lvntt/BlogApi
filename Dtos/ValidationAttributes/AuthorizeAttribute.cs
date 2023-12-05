// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;
//
// namespace BlogApi.Dtos.ValidationAttributes;
//
// [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
// public class AuthorizeAttribute : Attribute, IAuthorizationFilter
// {
//     public void OnAuthorization(AuthorizationFilterContext context)
//     {
//         var allowAnonymous = context.ActionDescriptor.EndpointMetadata
//             .OfType<AllowAnonymousAttribute>().Any();
//         if (allowAnonymous)
//         {
//             return;
//         }
//
//         var userId = (Guid?)context.HttpContext.Items["UserId"];
//         if (userId == null)
//         {
//             context.Result = new JsonResult(new
//             {
//                 type = "Error",
//                 status = StatusCodes.Status401Unauthorized,
//                 detail = "Unauthorized"
//             })
//             {
//                 StatusCode = StatusCodes.Status401Unauthorized
//             };
//         }
//     }
// }