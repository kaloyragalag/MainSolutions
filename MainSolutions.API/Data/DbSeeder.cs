using MainSolutions.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync(u => u.Email == "admin@mainsolutions.com"))
        {
            var admin = new User
            {
                Email = "admin@mainsolutions.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            Console.WriteLine("Seed: Admin user created.");
        }
        else
        {
            Console.WriteLine("Seed: Admin user already exists, skipping.");
        }
    }
}
