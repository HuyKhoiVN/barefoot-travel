using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Common
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(SysDbContext context)
        {
            // Ensure roles exist
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role 
                    { 
                        RoleName = "Admin", 
                        Description = "Administrator role with full access",
                        Active = true,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Role 
                    { 
                        RoleName = "User", 
                        Description = "Regular user role with limited access",
                        Active = true,
                        CreatedTime = DateTime.UtcNow
                    }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Create admin user if not exists
            if (!await context.Accounts.AnyAsync(a => a.Username == "admin"))
            {
                var adminRole = await context.Roles.FirstAsync(r => r.RoleName == "Admin");
                
                var adminUser = new Account
                {
                    Username = "admin",
                    FullName = "System Administrator",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Email = "admin@barefoottravel.com",
                    Phone = "+1234567890",
                    RoleId = adminRole.Id,
                    Active = true,
                    CreatedTime = DateTime.UtcNow
                };

                await context.Accounts.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }

            // Create sample user if not exists
            if (!await context.Accounts.AnyAsync(a => a.Username == "user"))
            {
                var userRole = await context.Roles.FirstAsync(r => r.RoleName == "User");
                
                var regularUser = new Account
                {
                    Username = "user",
                    FullName = "John Doe",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                    Email = "user@example.com",
                    Phone = "+0987654321",
                    RoleId = userRole.Id,
                    Active = true,
                    CreatedTime = DateTime.UtcNow
                };

                await context.Accounts.AddAsync(regularUser);
                await context.SaveChangesAsync();
            }
        }
    }
}
