using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public interface IEnrolmentService
    {
        Task<bool> ApplyAsync(int initiativeId, string volunteerId);
        Task<List<Enrolment>> GetEnrolmentsByVolunteerAsync(string volunteerId);
        Task<Initiative?> GetInitiativeWithEnrolmentsAsync(int initiativeId);
        Task<bool> UpdateStatusAsync(int enrolmentId, EnrolmentStatus status);
        Task<Enrolment?> GetEnrolmentByIdAsync(int enrolmentId);
        Task<string> EnsureCertificateCodeAsync(int enrolmentId);
    }
}
