using FleetCare_Pro.Models.Identity;
using Microsoft.AspNetCore.Identity;


namespace FleetCare_Pro.Data
{
  public static class DbInitializer
  {
      public static async Task InitializeAsync(
          RoleManager<IdentityRole> roleManager,
          UserManager<ApplicationUser> userManager)
      {
          // Roles
          string[] roles = { "Admin", "FleetManager", "Driver"};

          foreach (var role in roles)
          {
              if (!await roleManager.RoleExistsAsync(role))
              {
                  await roleManager.CreateAsync(
                      new IdentityRole(role));
              }
          }

          // Initial Admin
          const string adminEmail = "admin@fleetcare.com";
          const string adminPassword = "Admin@12345";

          var admin = await userManager.FindByEmailAsync(
              adminEmail);

          if (admin == null)
          {
              admin = new ApplicationUser
              {
                  FullName = "System Administrator",
                  EmployeeId = "ADMIN001",
                  UserName = adminEmail,
                  Email = adminEmail,
                  EmailConfirmed = true
              };

              var result = await userManager.CreateAsync(
                  admin,
                  adminPassword);

              if (!result.Succeeded)
              {
                  throw new Exception("Failed to create initial admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
              }
          }

          if (!await userManager.IsInRoleAsync(admin, "Admin"))
          {
              await userManager.AddToRoleAsync(admin,"Admin");
          }
      }
  }

}
