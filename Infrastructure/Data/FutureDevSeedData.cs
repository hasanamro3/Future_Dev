using Domain.Entites.Enums;
using Domain.Entites.Models;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class FutureDevSeedData
    {
        public static async Task Seed(FutureDbContext context)
        {
            if (!context.Roles.Any())
            {
                var adminRole = new Role
                {
                    Code = RoleEnum.Admin,
                    RoleName = "Admin"
                };

                var userRole = new Role
                {
                    Code = RoleEnum.User,
                    RoleName = "User"
                };

                await context.Roles.AddRangeAsync(adminRole, userRole);
                await context.SaveChangesAsync();
            }

            if (!context.Users.Any())
            {
                var adminRole = await context.Roles
                    .FirstAsync(r => r.Code == RoleEnum.Admin);

                var passwordHasher = new PasswordHasher<User>();

                var admin = new User
                {
                    FullName = "System Admin",
                    Email = "admin@system.dev",
                    PhoneNumber = "1234567890",
                    RoleId = adminRole.RoleId
                };

                admin.Password = passwordHasher.HashPassword(admin, "Admin@123");

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
            }

            if (!context.Categories.Any())
            {
                var category1 = new Category
                {   
                   Name = "IT"
                };
                var category2 = new Category
                {   
                   Name = "HR"
                };
                var category3= new Category
                {   
                   Name = "Sales"
                };
                var category4= new Category
                {   
                   Name = "Marketing"
                };

               

                await context.Categories.AddRangeAsync(category1, category2, category3, category4);
                await context.SaveChangesAsync();
            }

        }
    }

}

