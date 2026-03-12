using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Models;

namespace volunteerplatform.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(
            IServiceProvider serviceProvider,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ─── 1. ROLES ────────────────────────────────────────────────────────────
            foreach (var role in new[] { "SuperAdmin", "Admin", "Organizer", "Volunteer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ─── 2. SEED USERS ───────────────────────────────────────────────────────
            // Basic users to ensure the app is usable immediately
            var superAdmin = await EnsureUser(userManager, new ApplicationUser
            {
                UserName = "superadmin@volunteer.com",
                Email = "superadmin@volunteer.com",
                FullName = "Main SuperAdmin",
                EmailConfirmed = true,
                IsVerified = true
            }, "Super123!", "SuperAdmin");

            var admin = await EnsureUser(userManager, new ApplicationUser
            {
                UserName = "admin@volunteer.com",
                Email = "admin@volunteer.com",
                FullName = "System Administrator",
                EmailConfirmed = true,
                IsVerified = true
            }, "Admin123!", "Admin");

            // Regular Organizers
            var basicOrg1 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName = "org1@test.com",
                Email = "org1@test.com",
                FullName = "Ivan Petrov",
                OrganizationName = "Green Future Foundation",
                Location = "Sofia",
                EmailConfirmed = true,
                IsVerified = true
            }, "Org123!", "Organizer");

            var basicOrg2 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName = "org2@test.com",
                Email = "org2@test.com",
                FullName = "Maria Dimitrova",
                OrganizationName = "Social Care United",
                Location = "Plovdiv",
                EmailConfirmed = true,
                IsVerified = true
            }, "Org123!", "Organizer");

            // ─── 3. SEED INITIATIVES AND ACTIVITY ─────────────────────────────────────
            if (!context.Initiatives.Any())
            {
                var random = new Random();
                var cities = new[] { "Sofia", "Plovdiv", "Varna", "Burgas", "Ruse", "Stara Zagora", "Pleven", "Veliko Tarnovo", "Blagoevgrad" };
                
                var categoryTemplates = new[] {
                    new { 
                        Cat = "Environment", 
                        Titles = new[] { "Park Restoration", "Urban Tree Planting", "River Bank Cleanup", "Mountain Trail Maintenance", "Sustainable Community Garden", "Electronic Waste Collection" },
                        Skills = new[] { "Gardening", "Construction", "Endurance", "Manual Labor" },
                        Desc = "Help us preserve our natural beauty. We will be working on restoring local green spaces, planting native species, and cleaning up debris from sensitive ecological areas."
                    },
                    new { 
                        Cat = "Social Support", 
                        Titles = new[] { "Homeless Kitchen Helper", "Food Bank Distribution", "Senior Companion Program", "Refugee Support Center", "Toy Drive for Charity", "Clothing Distribution" },
                        Skills = new[] { "Empathy", "Cooking", "Organization", "Languages" },
                        Desc = "Make a direct impact on the lives of those in need. From preparing hot meals to providing companionship to the elderly, your presence matters."
                    },
                    new { 
                        Cat = "Education & Tech", 
                        Titles = new[] { "Coding for Kids", "Teach English Online", "Senior Digital Literacy", "Library Archive Scanning", "Science Fair Mentoring", "Youth Career Workshop" },
                        Skills = new[] { "IT", "Languages", "Teaching", "Public Speaking" },
                        Desc = "Knowledge is power. Join our educational initiatives to bridge the digital divide and provide learning opportunities for children and seniors alike."
                    },
                    new { 
                        Cat = "Animal Care", 
                        Titles = new[] { "Dog Shelter Volunteering", "Cat Cafe Socialization", "Wildlife Rehab Support", "Horse Therapy Assistant", "Vet Clinic Volunteer", "Stray Census Project" },
                        Skills = new[] { "Animal Care", "Patience", "Dog Handling", "First Aid" },
                        Desc = "Be a voice for the voiceless. Help our local animal shelters with daily care, socialization, and maintenance of their facilities."
                    },
                    new { 
                        Cat = "Arts & Culture", 
                        Titles = new[] { "Gallery Guide", "Theater Workshop", "Community Mural Painting", "Music Festival Support", "Heritage Site Cleanup", "Photography for NGOs" },
                        Skills = new[] { "Art", "Design", "Event Planning", "Photography" },
                        Desc = "Promote and preserve our local culture. Join us in organizing events, creating public art, or supporting local cultural institutions."
                    }
                };

                // Coordinates for Bulgarian cities with some variation
                var cityCoords = new Dictionary<string, (double Lat, double Lng)>
                {
                    { "Sofia", (42.6977, 23.3219) },
                    { "Plovdiv", (42.1354, 24.7453) },
                    { "Varna", (43.2141, 27.9147) },
                    { "Burgas", (42.5048, 27.4626) },
                    { "Ruse", (43.8356, 25.9657) },
                    { "Stara Zagora", (42.4258, 25.6345) },
                    { "Pleven", (43.4170, 24.6067) },
                    { "Veliko Tarnovo", (43.0757, 25.6172) },
                    { "Blagoevgrad", (42.0206, 23.0943) }
                };

                // Create a pool of volunteers for mass seeding
                var volunteers = new List<ApplicationUser>();
                for (int i = 1; i <= 50; i++)
                {
                    var email = $"volunteer{i}@example.com";
                    volunteers.Add(await EnsureUser(userManager, new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = GetRandomName(random),
                        Age = 18 + random.Next(40),
                        Location = cities[random.Next(cities.Length)],
                        Skills = string.Join(", ", Enumerable.Range(0, 3).Select(_ => GetRandomSkill(random))),
                        Availability = i % 2 == 0 ? "Weekends" : "Flexible",
                        EmailConfirmed = true
                    }, "Vol123!", "Volunteer"));
                }

                // Create Initiatives
                var organizers = new List<string> { basicOrg1.Id, basicOrg2.Id, admin.Id };
                for (int i = 1; i <= 60; i++)
                {
                    var template = categoryTemplates[random.Next(categoryTemplates.Length)];
                    var city = cities[random.Next(cities.Length)];
                    var coords = cityCoords[city];
                    var status = i <= 20 ? MissionStatus.Finished : (i <= 40 ? MissionStatus.Active : MissionStatus.Filled);

                    double latOffset = (random.NextDouble() - 0.5) * 0.08;
                    double lngOffset = (random.NextDouble() - 0.5) * 0.08;

                    var init = new Initiative
                    {
                        Title = $"{template.Titles[random.Next(template.Titles.Length)]} in {city}",
                        Description = $"{template.Desc} This initiative is hosted by our dedicated team in {city}. Participants will receive certifications and a welcome package.",
                        Location = city + ", Bulgaria",
                        Latitude = coords.Lat + latOffset,
                        Longitude = coords.Lng + lngOffset,
                        DateAndTime = DateTime.Now.AddDays(status == MissionStatus.Finished ? -random.Next(30, 90) : random.Next(10, 60)),
                        RequiredVolunteers = 5 + random.Next(25),
                        RequiredSkills = string.Join(", ", template.Skills.OrderBy(_ => random.Next()).Take(2)),
                        OrganizerId = organizers[random.Next(organizers.Count)],
                        Status = status,
                        TargetAmount = (i % 3 == 0) ? random.Next(1, 10) * 500 : null,
                        CurrentAmount = 0
                    };

                    if (status == MissionStatus.Finished && init.TargetAmount.HasValue)
                        init.CurrentAmount = init.TargetAmount.Value;
                    else if (status == MissionStatus.Active && init.TargetAmount.HasValue)
                        init.CurrentAmount = random.Next(0, (int)init.TargetAmount.Value / 2);

                    context.Initiatives.Add(init);
                }
                await context.SaveChangesAsync();

                // ─── 4. ENROLMENTS AND RANKINGS (LEADERBOARD) ───────────────────────────
                var savedMissions = await context.Initiatives.ToListAsync();
                var finishedMissions = savedMissions.Where(m => m.Status == MissionStatus.Finished).ToList();

                foreach (var vol in volunteers)
                {
                    // Create activity profile
                    int missionsCount = 0;
                    if (vol.UserName?.Contains("volunteer1") == true || vol.UserName?.Contains("volunteer10") == true || vol.UserName?.Contains("volunteer20") == true)
                        missionsCount = 30 + random.Next(10); // Potential Winners
                    else if (random.Next(10) > 5)
                        missionsCount = 2 + random.Next(8); // Regulars
                    else
                        missionsCount = 0; // Newbies

                    var selections = finishedMissions.OrderBy(_ => random.Next()).Take(missionsCount).ToList();
                    foreach (var m in selections)
                    {
                        context.Enrolments.Add(new Enrolment
                        {
                            InitiativeId = m.Id,
                            VolunteerId = vol.Id,
                            Status = EnrolmentStatus.Approved,
                            AppliedOn = m.DateAndTime.AddDays(-15),
                            CertificateCode = "VP-CERT-" + Guid.NewGuid().ToString()[..6].ToUpper()
                        });

                        // 50% chance to have a rating
                        if (random.Next(10) > 5)
                        {
                            context.Ratings.Add(new Rating
                            {
                                InitiativeId = m.Id,
                                VolunteerId = vol.Id,
                                OrganizerId = m.OrganizerId,
                                Score = 5,
                                Comment = "Extraordinary contribution. Very reliable and proactive."
                            });
                        }
                    }

                    // Add some donations
                    if (random.Next(10) > 7)
                    {
                        var activeMission = savedMissions.FirstOrDefault(m => m.Status == MissionStatus.Active && m.TargetAmount.HasValue);
                        if (activeMission != null)
                        {
                            context.Donations.Add(new Donation
                            {
                                InitiativeId = activeMission.Id,
                                DonorId = vol.Id,
                                Amount = 20 + (random.Next(10) * 10),
                                DonatedOn = DateTime.Now.AddDays(-random.Next(1, 14)),
                                Message = "Glad to support this cause!"
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();

                // Recalculate volunteer ratings
                foreach (var vol in volunteers)
                {
                    await UpdateRating(userManager, vol.Id, context);
                }
            }
        }

        // ─── UTILS ───────────────────────────────────────────────────────────────────

        private static async Task<ApplicationUser> EnsureUser(
            UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string password,
            string role)
        {
            var existing = await userManager.FindByEmailAsync(user.Email!);
            if (existing != null) return existing;

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);

            return user;
        }

        private static async Task UpdateRating(
            UserManager<ApplicationUser> userManager,
            string userId,
            ApplicationDbContext context)
        {
            var scores = await context.Ratings
                .Where(r => r.VolunteerId == userId)
                .Select(r => r.Score)
                .ToListAsync();

            if (!scores.Any()) return;

            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Rating = (int)Math.Round(scores.Average());
                await userManager.UpdateAsync(user);
            }
        }

        private static string GetRandomName(Random random)
        {
            var fn = new[] { "Ivan", "Maria", "Stefan", "Elena", "Dimitar", "Gergana", "Petar", "Alexandra", "Hristo", "Desislava", "Nikolay", "Viktoria", "Angel", "Katya", "Simeon", "Mila", "Kaloyan", "Raya", "Boris", "Nina" };
            var ln = new[] { "Petrov", "Ivanova", "Georgiev", "Dimitrova", "Stoyanov", "Nikolova", "Marinov", "Koleva", "Hristov", "Vasileva", "Todorov", "Petrova", "Yordanov", "Stoyanova", "Popov", "Angelova" };
            return $"{fn[random.Next(fn.Length)]} {ln[random.Next(ln.Length)]}";
        }

        private static string GetRandomSkill(Random random)
        {
            var skills = new[] { "Teaching", "IT", "Cooking", "First Aid", "Driving", "Social Media", "Art", "Languages", "Construction", "Animal Care", "Communication", "Organization", "Events", "Design", "Photography" };
            return skills[random.Next(skills.Length)];
        }
    }
}
