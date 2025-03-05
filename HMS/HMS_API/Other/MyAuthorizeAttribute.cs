using DBLayer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS_API
{
    public class MyAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Resolve your custom UserService from the request's DI container.
            var userService = context.HttpContext.RequestServices.GetService(typeof(UserService)) as UserService;

            // If there is no logged-in user, return Unauthorized.
            //if (userService?.LoggedUser == null)
            //{
            //    context.Result = new UnauthorizedResult();
            //    return;
            //}

            var claimsIdentity = context.HttpContext.User.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                //var userNameClaim = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

                // You can access other claims as needed
                // var roleClaim = claimsIdentity.FindFirst(ClaimTypes.Role)?.Value;

                // You can also set these claims in your UserService if needed
                //userService.LoggedUser.Id = userIdClaim;
                //userService.LoggedUser.UserName = userNameClaim;
                 if (string.IsNullOrEmpty(userIdClaim) || await userService.GetUserById(userIdClaim) == null)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}
