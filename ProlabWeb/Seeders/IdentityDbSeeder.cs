using Microsoft.AspNetCore.Identity;
using ProlabWeb.Data;
using Serilog;
using static ProlabWeb.Properties.Constants;

namespace ProlabWeb.Seeders
{
    public class IdentityDbSeeder
    {
        public static async Task SeedRolesAndAdminAccountAsync(UserManager<ProlabIdentityUser> userManager, RoleManager<IdentityRole> roleManager, string adminEmail)
        {
            //seed default roles
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            if (!await roleManager.RoleExistsAsync(Roles.Caisse))
                await roleManager.CreateAsync(new IdentityRole(Roles.Caisse));
            if (!await roleManager.RoleExistsAsync(Roles.GestionAnalyse))
                await roleManager.CreateAsync(new IdentityRole(Roles.GestionAnalyse));
            if (!await roleManager.RoleExistsAsync(Roles.Biologiste))
                await roleManager.CreateAsync(new IdentityRole(Roles.Biologiste));
            if (!await roleManager.RoleExistsAsync(Roles.Technicien))
                await roleManager.CreateAsync(new IdentityRole(Roles.Technicien));
            if (!await roleManager.RoleExistsAsync(Roles.GestionResultat))
                await roleManager.CreateAsync(new IdentityRole(Roles.GestionResultat));
            if (!await roleManager.RoleExistsAsync(Roles.TopManagment))
                await roleManager.CreateAsync(new IdentityRole(Roles.TopManagment));
            if (!await roleManager.RoleExistsAsync("Gestionnaire"))
                await roleManager.CreateAsync(new IdentityRole("Gestionnaire"));

            if (string.IsNullOrEmpty(adminEmail))
            {
                Log.Warning("AdminEmail not provided");
                return;
            }
            //Seed admin User
            var administrator = new ProlabIdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                MustChangePassword = true,
                TemporaryPassword = "P@ssw0rd",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var user = await userManager.FindByNameAsync(administrator.UserName);

            if (user == null)
            {
                await userManager.CreateAsync(administrator, "P@ssw0rd");
                await userManager.AddToRoleAsync(administrator, Roles.Admin.ToString());
            }

        }
    }
}
