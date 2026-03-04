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
            // ─── 1. ROLES ────────────────────────────────────────────────────────────
            foreach (var role in new[] { "Admin", "Organizer", "Volunteer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ─── 2. USERS ────────────────────────────────────────────────────────────
            var admin = await EnsureUser(userManager, new ApplicationUser
            {
                UserName        = "admin@volunteer.com",
                Email           = "admin@volunteer.com",
                FullName        = "System Administrator",
                EmailConfirmed  = true,
                IsVerified      = true
            }, "Admin123!", "Admin");

            // — Organizers —
            var org1 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName         = "org1@test.com",
                Email            = "org1@test.com",
                FullName         = "Ivan Petrov",
                OrganizationName = "GreenFuture NGO",
                Location         = "Sofia",
                EmailConfirmed   = true,
                IsVerified       = true
            }, "Org123!", "Organizer");

            var org2 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName         = "org2@test.com",
                Email            = "org2@test.com",
                FullName         = "Maria Dimitrova",
                OrganizationName = "Helping Hands Foundation",
                Location         = "Plovdiv",
                EmailConfirmed   = true,
                IsVerified       = true
            }, "Org123!", "Organizer");

            var org3 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName         = "org3@test.com",
                Email            = "org3@test.com",
                FullName         = "Stefan Georgiev",
                OrganizationName = "Animal Rescue Bulgaria",
                Location         = "Varna",
                EmailConfirmed   = true,
                IsVerified       = true
            }, "Org123!", "Organizer");

            // — Volunteers —
            var vol1 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName       = "vol1@test.com",
                Email          = "vol1@test.com",
                FullName       = "Alice Kowalski",
                Age            = 24,
                Skills         = "Teaching, Events, Cooking",
                Availability   = "Weekends",
                Location       = "Sofia",
                EmailConfirmed = true
            }, "Vol123!", "Volunteer");

            var vol2 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName       = "vol2@test.com",
                Email          = "vol2@test.com",
                FullName       = "Boris Ivanov",
                Age            = 31,
                Skills         = "Construction, Driving, First Aid",
                Availability   = "Weekdays",
                Location       = "Plovdiv",
                EmailConfirmed = true
            }, "Vol123!", "Volunteer");

            var vol3 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName       = "vol3@test.com",
                Email          = "vol3@test.com",
                FullName       = "Clara Hristova",
                Age            = 19,
                Skills         = "Social Media, Design",
                Availability   = "Flexible",
                Location       = "Varna",
                EmailConfirmed = true
            }, "Vol123!", "Volunteer");

            var vol4 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName       = "vol4@test.com",
                Email          = "vol4@test.com",
                FullName       = "Dimitar Stoyanov",
                Age            = 28,
                Skills         = "Dog handling, Animal care",
                Availability   = "Weekends",
                Location       = "Varna",
                EmailConfirmed = true
            }, "Vol123!", "Volunteer");

            var vol5 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName       = "vol5@test.com",
                Email          = "vol5@test.com",
                FullName       = "Elena Nikolova",
                Age            = 22,
                Skills         = "Teaching, Languages, Events",
                Availability   = "Flexible",
                Location       = "Sofia",
                EmailConfirmed = true
            }, "Vol123!", "Volunteer");

            var vol6 = await EnsureUser(userManager, new ApplicationUser
            {
                UserName       = "vol6@test.com",
                Email          = "vol6@test.com",
                FullName       = "Filip Marinov",
                Age            = 35,
                Skills         = "IT, Coding, Networking",
                Availability   = "Evenings",
                Location       = "Sofia",
                EmailConfirmed = true
            }, "Vol123!", "Volunteer");

            // ─── 3. INITIATIVES ───────────────────────────────────────────────────────
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!context.Initiatives.Any())
            {
                var now = DateTime.Now;

                var initiatives = new List<Initiative>
                {
                    // ── Active ──
                    new Initiative
                    {
                        Title              = "City Park Cleanup",
                        Description        = "Join us for a morning of cleaning up our local city park. We provide bags, gloves and refreshments for everyone who helps!",
                        Location           = "Южен парк, Sofia",
                        Latitude           = 42.6688,
                        Longitude          = 23.3136,
                        DateAndTime        = now.AddDays(8),
                        RequiredVolunteers = 25,
                        RequiredSkills     = "None — all welcome",
                        OrganizerId        = org1.Id,
                        Status             = MissionStatus.Active,
                        TargetAmount       = 500,
                        CurrentAmount      = 200
                    },
                    new Initiative
                    {
                        Title              = "Food Bank Support",
                        Description        = "Help sort and pack food donations at our central warehouse. Every box you pack feeds a family in need.",
                        Location           = "Food Bank, Plovdiv",
                        Latitude           = 42.1354,
                        Longitude          = 24.7453,
                        DateAndTime        = now.AddDays(15),
                        RequiredVolunteers = 12,
                        RequiredSkills     = "Organization",
                        OrganizerId        = org2.Id,
                        Status             = MissionStatus.Active,
                        TargetAmount       = 1000,
                        CurrentAmount      = 630
                    },
                    new Initiative
                    {
                        Title              = "Blood Donation Drive",
                        Description        = "We're organizing a blood donation campaign across the city. Volunteers needed to manage registration and assist donors.",
                        Location           = "УМБАЛ Александровска, Sofia",
                        Latitude           = 42.6985,
                        Longitude          = 23.3322,
                        DateAndTime        = now.AddDays(5),
                        RequiredVolunteers = 8,
                        RequiredSkills     = "First Aid, Communication",
                        OrganizerId        = org1.Id,
                        Status             = MissionStatus.Active,
                        TargetAmount       = null,
                        CurrentAmount      = 0
                    },
                    new Initiative
                    {
                        Title              = "Coastal Beach Cleanup",
                        Description        = "Help us restore the beauty of the Black Sea coastline. Bring your energy — we supply the equipment.",
                        Location           = "Централен плаж, Varna",
                        Latitude           = 43.2049,
                        Longitude          = 27.9118,
                        DateAndTime        = now.AddDays(20),
                        RequiredVolunteers = 40,
                        RequiredSkills     = "None",
                        OrganizerId        = org3.Id,
                        Status             = MissionStatus.Active,
                        TargetAmount       = 800,
                        CurrentAmount      = 320
                    },
                    new Initiative
                    {
                        Title              = "Digital Literacy for Seniors",
                        Description        = "Teach elderly residents how to use smartphones, video calls, and the internet safely. Patience is all you need!",
                        Location           = "Домашен социален патронаж, Sofia",
                        Latitude           = 42.7120,
                        Longitude          = 23.2880,
                        DateAndTime        = now.AddDays(10),
                        RequiredVolunteers = 6,
                        RequiredSkills     = "IT, Teaching",
                        OrganizerId        = org1.Id,
                        Status             = MissionStatus.Active,
                        TargetAmount       = 300,
                        CurrentAmount      = 150
                    },

                    // ── Filled ──
                    new Initiative
                    {
                        Title              = "Animal Shelter Walk",
                        Description        = "Take shelter dogs for a walk while we deep-clean their living spaces. Pure love guaranteed.",
                        Location           = "Приют за бездомни животни, Varna",
                        Latitude           = 43.2141,
                        Longitude          = 27.9145,
                        DateAndTime        = now.AddDays(3),
                        RequiredVolunteers = 5,
                        RequiredSkills     = "Dog handling",
                        OrganizerId        = org3.Id,
                        Status             = MissionStatus.Filled,
                        TargetAmount       = 200,
                        CurrentAmount      = 200
                    },
                    new Initiative
                    {
                        Title              = "Children's Art Workshop",
                        Description        = "Run creative art sessions for children from underprivileged backgrounds. Art supplies provided.",
                        Location           = "НЧ Напредък, Plovdiv",
                        Latitude           = 42.1496,
                        Longitude          = 24.7500,
                        DateAndTime        = now.AddDays(6),
                        RequiredVolunteers = 4,
                        RequiredSkills     = "Art, Teaching",
                        OrganizerId        = org2.Id,
                        Status             = MissionStatus.Filled,
                        TargetAmount       = 400,
                        CurrentAmount      = 400
                    },

                    // ── Finished ──
                    new Initiative
                    {
                        Title              = "Tree Planting Day",
                        Description        = "We planted over 200 trees across the Vitosha mountain foothills. Thank you to everyone who joined!",
                        Location           = "Витоша, Sofia",
                        Latitude           = 42.5900,
                        Longitude          = 23.2800,
                        DateAndTime        = now.AddDays(-30),
                        RequiredVolunteers = 30,
                        RequiredSkills     = "None",
                        OrganizerId        = org1.Id,
                        Status             = MissionStatus.Finished,
                        TargetAmount       = 600,
                        CurrentAmount      = 600
                    },
                    new Initiative
                    {
                        Title              = "Elderly Home Visit",
                        Description        = "We visited two elderly care homes in Plovdiv, bringing gifts, music, and company to over 80 residents.",
                        Location           = "Дом за стари хора, Plovdiv",
                        Latitude           = 42.1500,
                        Longitude          = 24.7400,
                        DateAndTime        = now.AddDays(-14),
                        RequiredVolunteers = 10,
                        RequiredSkills     = "Communication, Empathy",
                        OrganizerId        = org2.Id,
                        Status             = MissionStatus.Finished,
                        TargetAmount       = 300,
                        CurrentAmount      = 300
                    },
                    new Initiative
                    {
                        Title              = "School Supply Drive",
                        Description        = "Collected and distributed school supplies to 120 children from low-income families before the school year started.",
                        Location           = "ОУ Васил Левски, Sofia",
                        Latitude           = 42.6800,
                        Longitude          = 23.3200,
                        DateAndTime        = now.AddDays(-45),
                        RequiredVolunteers = 15,
                        RequiredSkills     = "Organization, Driving",
                        OrganizerId        = org1.Id,
                        Status             = MissionStatus.Finished,
                        TargetAmount       = 1200,
                        CurrentAmount      = 1200
                    }
                };

                context.Initiatives.AddRange(initiatives);
                await context.SaveChangesAsync();

                // Reload to get assigned IDs
                var saved = await context.Initiatives.ToListAsync();
                var cityPark      = saved.First(i => i.Title == "City Park Cleanup");
                var foodBank      = saved.First(i => i.Title == "Food Bank Support");
                var bloodDrive    = saved.First(i => i.Title == "Blood Donation Drive");
                var beachCleanup  = saved.First(i => i.Title == "Coastal Beach Cleanup");
                var digitalLit    = saved.First(i => i.Title == "Digital Literacy for Seniors");
                var shelterWalk   = saved.First(i => i.Title == "Animal Shelter Walk");
                var artWorkshop   = saved.First(i => i.Title == "Children's Art Workshop");
                var treePlanting  = saved.First(i => i.Title == "Tree Planting Day");
                var elderlyVisit  = saved.First(i => i.Title == "Elderly Home Visit");
                var supplyDrive   = saved.First(i => i.Title == "School Supply Drive");

                // ─── 4. ENROLMENTS ───────────────────────────────────────────────────
                var enrolments = new List<Enrolment>
                {
                    // Active initiatives — pending/approved
                    new Enrolment { InitiativeId = cityPark.Id,     VolunteerId = vol1.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-3) },
                    new Enrolment { InitiativeId = cityPark.Id,     VolunteerId = vol2.Id, Status = EnrolmentStatus.Pending,  AppliedOn = now.AddDays(-2) },
                    new Enrolment { InitiativeId = cityPark.Id,     VolunteerId = vol5.Id, Status = EnrolmentStatus.Pending,  AppliedOn = now.AddDays(-1) },
                    new Enrolment { InitiativeId = foodBank.Id,     VolunteerId = vol2.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-5) },
                    new Enrolment { InitiativeId = foodBank.Id,     VolunteerId = vol3.Id, Status = EnrolmentStatus.Rejected, AppliedOn = now.AddDays(-6) },
                    new Enrolment { InitiativeId = bloodDrive.Id,   VolunteerId = vol1.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-4) },
                    new Enrolment { InitiativeId = bloodDrive.Id,   VolunteerId = vol6.Id, Status = EnrolmentStatus.Pending,  AppliedOn = now.AddDays(-2) },
                    new Enrolment { InitiativeId = beachCleanup.Id, VolunteerId = vol3.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-7) },
                    new Enrolment { InitiativeId = digitalLit.Id,   VolunteerId = vol6.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-3) },

                    // Filled initiatives
                    new Enrolment { InitiativeId = shelterWalk.Id,  VolunteerId = vol4.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-8) },
                    new Enrolment { InitiativeId = artWorkshop.Id,  VolunteerId = vol5.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-9) },
                    new Enrolment { InitiativeId = artWorkshop.Id,  VolunteerId = vol3.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-10) },

                    // Finished initiatives — with certificates
                    new Enrolment { InitiativeId = treePlanting.Id, VolunteerId = vol1.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-35), CertificateCode = "VP-" + Guid.NewGuid().ToString()[..8].ToUpper() },
                    new Enrolment { InitiativeId = treePlanting.Id, VolunteerId = vol2.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-36), CertificateCode = "VP-" + Guid.NewGuid().ToString()[..8].ToUpper() },
                    new Enrolment { InitiativeId = elderlyVisit.Id, VolunteerId = vol5.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-20), CertificateCode = "VP-" + Guid.NewGuid().ToString()[..8].ToUpper() },
                    new Enrolment { InitiativeId = elderlyVisit.Id, VolunteerId = vol3.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-21), CertificateCode = "VP-" + Guid.NewGuid().ToString()[..8].ToUpper() },
                    new Enrolment { InitiativeId = supplyDrive.Id,  VolunteerId = vol2.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-50), CertificateCode = "VP-" + Guid.NewGuid().ToString()[..8].ToUpper() },
                    new Enrolment { InitiativeId = supplyDrive.Id,  VolunteerId = vol6.Id, Status = EnrolmentStatus.Approved, AppliedOn = now.AddDays(-52), CertificateCode = "VP-" + Guid.NewGuid().ToString()[..8].ToUpper() },
                };

                context.Enrolments.AddRange(enrolments);
                await context.SaveChangesAsync();

                // ─── 5. DONATIONS ─────────────────────────────────────────────────────
                var donations = new List<Donation>
                {
                    new Donation { InitiativeId = cityPark.Id,     DonorId = vol1.Id, Amount = 50,  DonatedOn = now.AddDays(-5),  Message = "Happy to help keep our parks clean!" },
                    new Donation { InitiativeId = cityPark.Id,     DonorId = vol5.Id, Amount = 150, DonatedOn = now.AddDays(-3),  Message = "Great cause, keep it up!" },
                    new Donation { InitiativeId = foodBank.Id,     DonorId = vol2.Id, Amount = 200, DonatedOn = now.AddDays(-8),  Message = "No one should go hungry." },
                    new Donation { InitiativeId = foodBank.Id,     DonorId = null,    Amount = 430, DonatedOn = now.AddDays(-4),  Message = "Anonymous donation." },
                    new Donation { InitiativeId = beachCleanup.Id, DonorId = vol3.Id, Amount = 120, DonatedOn = now.AddDays(-10), Message = "Love the Black Sea, let's protect it!" },
                    new Donation { InitiativeId = beachCleanup.Id, DonorId = vol6.Id, Amount = 200, DonatedOn = now.AddDays(-6),  Message = null },
                    new Donation { InitiativeId = treePlanting.Id, DonorId = vol1.Id, Amount = 300, DonatedOn = now.AddDays(-40), Message = "One tree at a time." },
                    new Donation { InitiativeId = supplyDrive.Id,  DonorId = vol4.Id, Amount = 500, DonatedOn = now.AddDays(-55), Message = "Every child deserves to learn." },
                };

                context.Donations.AddRange(donations);
                await context.SaveChangesAsync();

                // ─── 6. RATINGS ───────────────────────────────────────────────────────
                var ratings = new List<Rating>
                {
                    new Rating { InitiativeId = treePlanting.Id, VolunteerId = vol1.Id, OrganizerId = org1.Id, Score = 5, Comment = "Alice was amazing — organized, motivating and worked the whole day!" },
                    new Rating { InitiativeId = treePlanting.Id, VolunteerId = vol2.Id, OrganizerId = org1.Id, Score = 4, Comment = "Boris did a great job. Very reliable." },
                    new Rating { InitiativeId = elderlyVisit.Id, VolunteerId = vol5.Id, OrganizerId = org2.Id, Score = 5, Comment = "Elena was incredibly warm and kind with the residents." },
                    new Rating { InitiativeId = elderlyVisit.Id, VolunteerId = vol3.Id, OrganizerId = org2.Id, Score = 4, Comment = "Clara was creative and engaged everyone nicely." },
                    new Rating { InitiativeId = supplyDrive.Id,  VolunteerId = vol2.Id, OrganizerId = org1.Id, Score = 5, Comment = "Boris drove all day without complaint. Excellent!" },
                    new Rating { InitiativeId = supplyDrive.Id,  VolunteerId = vol6.Id, OrganizerId = org1.Id, Score = 3, Comment = "Filip was helpful but arrived late. Still a good effort." },
                };

                context.Ratings.AddRange(ratings);
                await context.SaveChangesAsync();

                // ─── 7. UPDATE VOLUNTEER RATINGS (average score) ──────────────────────
                await UpdateVolunteerRating(userManager, vol1.Id, context);
                await UpdateVolunteerRating(userManager, vol2.Id, context);
                await UpdateVolunteerRating(userManager, vol3.Id, context);
                await UpdateVolunteerRating(userManager, vol5.Id, context);
                await UpdateVolunteerRating(userManager, vol6.Id, context);
            }
        }

        // ─── HELPERS ─────────────────────────────────────────────────────────────────

        private static async Task<ApplicationUser> EnsureUser(
            UserManager<ApplicationUser> userManager,
            ApplicationUser template,
            string password,
            string role)
        {
            var existing = await userManager.FindByEmailAsync(template.Email!);
            if (existing != null) return existing;

            var result = await userManager.CreateAsync(template, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(template, role);

            return template;
        }

        private static async Task UpdateVolunteerRating(
            UserManager<ApplicationUser> userManager,
            string volunteerId,
            ApplicationDbContext context)
        {
            var scores = await context.Ratings
                .Where(r => r.VolunteerId == volunteerId)
                .Select(r => r.Score)
                .ToListAsync();

            if (!scores.Any()) return;

            var user = await userManager.FindByIdAsync(volunteerId);
            if (user == null) return;

            user.Rating = (int)Math.Round(scores.Average());
            await userManager.UpdateAsync(user);
        }
    }
}
