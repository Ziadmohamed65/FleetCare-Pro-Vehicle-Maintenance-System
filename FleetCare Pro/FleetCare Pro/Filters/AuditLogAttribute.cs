using FleetCare_Pro.Data;
using FleetCare_Pro.Models.Domain;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FleetCare_Pro.Filters
{
    public class AuditLogAttribute : ActionFilterAttribute
    {
        private readonly string _action;

        public AuditLogAttribute(string action)
        {
            _action = action;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // Execute the controller action first
            var resultContext = await next();

            // Only log if the action completed successfully
            if (resultContext.Exception == null)
            {
                var dbContext =
                    context.HttpContext.RequestServices
                    .GetRequiredService<FleetCareDbContext>();

                var userId =
                    context.HttpContext.User.FindFirst(
                        System.Security.Claims.ClaimTypes.NameIdentifier
                    )?.Value;

                var controller =
                    context.RouteData.Values["controller"]?.ToString();

                var action =
                    context.RouteData.Values["action"]?.ToString();

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = _action,
                    Details = $"{controller}/{action}",
                    Timestamp = DateTime.UtcNow
                };

                dbContext.AuditLogs.Add(auditLog);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}