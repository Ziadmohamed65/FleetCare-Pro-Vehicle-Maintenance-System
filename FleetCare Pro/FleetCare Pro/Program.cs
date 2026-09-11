using FleetCare_Pro.Data;
using FleetCare_Pro.Data;
using FleetCare_Pro.Middleware;
using FleetCare_Pro.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FleetCare_Pro
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<FleetCareDbContext>(options =>
                 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services
                   .AddIdentity<ApplicationUser, IdentityRole>()
                   .AddEntityFrameworkStores<FleetCareDbContext>()
                   .AddDefaultTokenProviders();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("FleetManagerOnly", policy =>
                    policy.RequireRole("FleetManager"));

                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("FleetManagement", policy =>
                    policy.RequireRole("Admin", "FleetManager"));

                options.AddPolicy("AllUsers", policy =>
                    policy.RequireRole("Admin", "FleetManager", "Driver"));
            });


            var app = builder.Build();


            // Seed Identity roles and initial Admin
            using (var scope = app.Services.CreateScope())
            {
                var roleManager =
                    scope.ServiceProvider
                        .GetRequiredService<RoleManager<IdentityRole>>();

                var userManager =
                    scope.ServiceProvider
                        .GetRequiredService<UserManager<ApplicationUser>>();

                DbInitializer.InitializeAsync(
                    roleManager,
                    userManager)
                    .GetAwaiter()
                    .GetResult();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<MaintenanceModeMiddleware>();


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
