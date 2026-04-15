namespace volunteerplatform.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendVolunteerApprovalEmailAsync(string email, string fullName, string initiativeTitle);
        Task SendNewInitiativeNotificationAsync(string email, string fullName, string initiativeTitle, string location);
    }
}
