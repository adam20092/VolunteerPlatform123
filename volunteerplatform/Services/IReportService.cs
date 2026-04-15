namespace volunteerplatform.Services
{
    public interface IReportService
    {
        Task<byte[]> GenerateInitiativesCsvAsync();
        Task<string> GenerateCertificateHtmlAsync(string volunteerName, string initiativeTitle, string date, string code);
    }
}
