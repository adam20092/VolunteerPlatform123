using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public class EnrolmentService : IEnrolmentService
    {
        private readonly ApplicationDbContext _context;

        public EnrolmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ApplyAsync(int initiativeId, string volunteerId)
        {
            var exists = await _context.Enrolments
                .AnyAsync(e => e.InitiativeId == initiativeId && e.VolunteerId == volunteerId);

            if (exists) return false;

            var enrolment = new Enrolment
            {
                InitiativeId = initiativeId,
                VolunteerId = volunteerId,
                Status = EnrolmentStatus.Pending,
                AppliedOn = DateTime.Now
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Enrolment>> GetEnrolmentsByVolunteerAsync(string volunteerId)
        {
            return await _context.Enrolments
                .Include(e => e.Initiative)
                .Where(e => e.VolunteerId == volunteerId)
                .OrderByDescending(e => e.AppliedOn)
                .ToListAsync();
        }

        public async Task<Initiative?> GetInitiativeWithEnrolmentsAsync(int initiativeId)
        {
            return await _context.Initiatives
                .Include(i => i.Enrolments!).ThenInclude(e => e.Volunteer)
                .FirstOrDefaultAsync(i => i.Id == initiativeId);
        }

        public async Task<bool> UpdateStatusAsync(int enrolmentId, EnrolmentStatus status)
        {
            var enrolment = await _context.Enrolments.FindAsync(enrolmentId);
            if (enrolment == null) return false;

            enrolment.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Enrolment?> GetEnrolmentByIdAsync(int enrolmentId)
        {
            return await _context.Enrolments
                .Include(e => e.Initiative).ThenInclude(i => i!.Organizer)
                .Include(e => e.Volunteer)
                .FirstOrDefaultAsync(e => e.Id == enrolmentId);
        }

        public async Task<string> EnsureCertificateCodeAsync(int enrolmentId)
        {
            var enrolment = await _context.Enrolments.FindAsync(enrolmentId);
            if (enrolment == null) return string.Empty;

            if (string.IsNullOrEmpty(enrolment.CertificateCode))
            {
                enrolment.CertificateCode = "VP-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                await _context.SaveChangesAsync();
            }

            return enrolment.CertificateCode;
        }
    }
}
