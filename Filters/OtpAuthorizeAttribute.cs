using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PetAdoptionManagementSystem.Filters
{
    public class OtpAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var isOtpVerified = context.HttpContext.Session.GetString("IsOtpVerified");
            if (string.IsNullOrEmpty(isOtpVerified) || isOtpVerified != "true")
            {
                // Redirect to an intermediate OTP-required info page instead of straight to Login
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("OtpRequired", "Account", new { returnUrl });
            }
            
            base.OnActionExecuting(context);
        }
    }
}
