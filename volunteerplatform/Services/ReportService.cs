using Microsoft.EntityFrameworkCore;
using System.Text;
using volunteerplatform.Data;

namespace volunteerplatform.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateInitiativesCsvAsync()
        {
            var initiatives = await _context.Initiatives
                .Include(i => i.Organizer)
                .OrderByDescending(i => i.DateAndTime)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Id,Title,Location,Date,Organizer,Status,Enrolments,TargetAmount,CurrentAmount");

            foreach (var item in initiatives)
            {
                var enrolmentsCount = await _context.Enrolments.CountAsync(e => e.InitiativeId == item.Id);
                csv.AppendLine($"{item.Id},\"{item.Title}\",\"{item.Location}\",{item.DateAndTime:yyyy-MM-dd HH:mm},{item.Organizer?.FullName},{item.Status},{enrolmentsCount},{item.TargetAmount},{item.CurrentAmount}");
            }

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        }

        public async Task<string> GenerateCertificateHtmlAsync(string volunteerName, string initiativeTitle, string date, string code)
        {
            // Simple but elegant HTML certificate that prints well to PDF
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; text-align: center; padding: 50px; border: 20px solid #0d6efd; margin: 0; }}
                    .container {{ border: 5px solid #0d6efd; padding: 40px; }}
                    h1 {{ color: #0d6efd; font-size: 48px; margin-bottom: 20px; }}
                    h2 {{ font-size: 32px; margin-top: 40px; }}
                    .name {{ font-size: 40px; font-weight: bold; border-bottom: 2px solid #333; display: inline-block; padding: 0 40px; margin: 20px 0; }}
                    .details {{ font-size: 20px; margin: 30px 0; line-height: 1.6; }}
                    .footer {{ margin-top: 60px; font-size: 16px; color: #666; }}
                    .code {{ font-family: monospace; font-weight: bold; color: #333; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h1>CERTIFICATE</h1>
                    <p style='font-size: 24px;'>OF APPRECIATION</p>
                    <div style='margin-top: 40px;'>This certificate is proudly presented to</div>
                    <div class='name'>{volunteerName}</div>
                    <div class='details'>
                        For their outstanding contribution and voluntary service in the initiative:<br/>
                        <b style='font-size: 26px;'>{initiativeTitle}</b><br/>
                        completed on {date}
                    </div>
                    <div class='footer'>
                        <p>VolunteerPlatform Community Team</p>
                        <p>Validation Code: <span class='code'>{code}</span></p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}
