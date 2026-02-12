using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using volunteerplatform.Models;

namespace volunteerplatform.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            string[] roleNames = { "Admin", "Organizer", "Volunteer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@volunteer.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Organizer
            var organizerEmail = "org@test.com";
            var organizerUser = await userManager.FindByEmailAsync(organizerEmail);
            if (organizerUser == null)
            {
                organizerUser = new ApplicationUser
                {
                    UserName = organizerEmail,
                    Email = organizerEmail,
                    FullName = "John Organizer",
                    OrganizationName = "Helping Hands NGO",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(organizerUser, "Org123!");
                await userManager.AddToRoleAsync(organizerUser, "Organizer");
            }

            // Seed Volunteer
            var volunteerEmail = "volunteer@test.com";
            var volunteerUser = await userManager.FindByEmailAsync(volunteerEmail);
            if (volunteerUser == null)
            {
                volunteerUser = new ApplicationUser
                {
                    UserName = volunteerEmail,
                    Email = volunteerEmail,
                    FullName = "Alice Volunteer",
                    Skills = "Teaching, Events",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(volunteerUser, "Vol123!");
                await userManager.AddToRoleAsync(volunteerUser, "Volunteer");
            }

            // Seed Initiatives
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (!context.Initiatives.Any())
            {
                var initiatives = new List<Initiative>
                {
                    new Initiative
                    {
                        Title = "City Park Cleanup",
                        Description = "Join us for a morning of cleaning up our local city park. We provide the bags and gloves!",
                        Location = "Central Park, Sofia",
                        DateAndTime = DateTime.Now.AddDays(7),
                        RequiredVolunteers = 20,
                        RequiredSkills = "None",
                        OrganizerId = organizerUser.Id,
                        Status = MissionStatus.Active,
                        Latitude = 42.688,
                        Longitude = 23.336
                    },
                    new Initiative
                    {
                        Title = "Food Bank Support",
                        Description = "Help sort and pack food donations for families in need.",
                        Location = "Food Bank Warehouse, Plovdiv",
                        DateAndTime = DateTime.Now.AddDays(14),
                        RequiredVolunteers = 10,
                        RequiredSkills = "Organization",
                        OrganizerId = organizerUser.Id,
                        Status = MissionStatus.Active,
                        Latitude = 42.135,
                        Longitude = 24.745
                    },
                    new Initiative
                    {
                        Title = "Animal Shelter Walk",
                        Description = "Take some of our furry friends for a walk while the shelter is being cleaned.",
                        Location = "Safe Haven Shelter, Varna",
                        DateAndTime = DateTime.Now.AddDays(3),
                        RequiredVolunteers = 5,
                        RequiredSkills = "Dog handling",
                        OrganizerId = organizerUser.Id,
                        Status = MissionStatus.Active,
                        Latitude = 43.214,
                        Longitude = 27.914
                    }
                };

                context.Initiatives.AddRange(initiatives);
                await context.SaveChangesAsync();
            }
        }
    }
}
