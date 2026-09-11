namespace FleetCare_Pro.Middleware
{
    public class MaintenanceModeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public MaintenanceModeMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            bool isMaintenanceMode =
                _configuration.GetValue<bool>("IsMaintenanceMode");

            if (isMaintenanceMode)
            {
                var path = context.Request.Path;

                // Allow the maintenance page itself
                if (!path.StartsWithSegments("/Home/Maintenance"))
                {
                    context.Response.Redirect("/Home/Maintenance");
                    return;
                }
            }

            await _next(context);
        }
    }
}