namespace volunteerplatform.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For a diploma project, we'll simulate sending by logging and printing to console
            // In a real app, you'd use SendGrid, MailKit or SMTP here.
            
            _logger.LogInformation($"[EMAIL SIMULATION] To: {email}, Subject: {subject}");
            
            // Console print for visibility during defense
            await Console.Out.WriteLineAsync("\n" + new string('-', 20));
            await Console.Out.WriteLineAsync($"🕊️ EMAIL SENT TO: {email}");
            await Console.Out.WriteLineAsync($"Subject: {subject}");
            await Console.Out.WriteLineAsync($"Message: {htmlMessage}");
            await Console.Out.WriteLineAsync(new string('-', 20) + "\n");
        }

        public async Task SendVolunteerApprovalEmailAsync(string email, string fullName, string initiativeTitle)
        {
            var subject = "Congratulations! Your application is approved";
            var message = $@"
                <h3>Hello {fullName},</h3>
                <p>We are happy to inform you that your application for the initiative <b>'{initiativeTitle}'</b> has been approved!</p>
                <p>Thank you for choosing to make a difference. We look forward to seeing you there.</p>
                <p>Best regards,<br/>The Volunteer Platform Team</p>";
            
            await SendEmailAsync(email, subject, message);
        }

        public async Task SendNewInitiativeNotificationAsync(string email, string fullName, string initiativeTitle, string location)
        {
            var subject = "New Initiative in your area!";
            var message = $@"
                <h3>Hello {fullName},</h3>
                <p>A new initiative <b>'{initiativeTitle}'</b> just started in <b>{location}</b>.</p>
                <p>Check it out and join the mission!</p>
                <p>Best regards,<br/>The Volunteer Platform Team</p>";

            await SendEmailAsync(email, subject, message);
        }
    }
}
