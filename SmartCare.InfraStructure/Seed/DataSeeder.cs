using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SmartCare.InfraStructure.Seed
{
    public static class RoleSeeder
    {
        private static readonly string[] DefaultRoles = new[]
        {
            "CLIENT",
            "DASHBOARD_ADMIN",
            "OWNER"
        };

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
