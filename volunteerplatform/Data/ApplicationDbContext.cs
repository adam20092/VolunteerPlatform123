using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Models;

namespace volunteerplatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Initiative> Initiatives { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Donation> Donations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Enrolment>()
                .HasOne(e => e.Volunteer)
                .WithMany()
                .HasForeignKey(e => e.VolunteerId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade cycle

            builder.Entity<Enrolment>()
                .HasOne(e => e.Initiative)
                .WithMany(i => i.Enrolments)
                .HasForeignKey(e => e.InitiativeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rating>()
                .HasOne(r => r.Volunteer)
                .WithMany()
                .HasForeignKey(r => r.VolunteerId)
                .OnDelete(DeleteBehavior.NoAction); 
        }
    }
}
