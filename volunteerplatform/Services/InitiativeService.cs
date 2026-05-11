using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public class InitiativeService : IInitiativeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        
        private static readonly Dictionary<string, string> BulgarianToEnglishMapping = new(StringComparer.OrdinalIgnoreCase)
        {
            // Categories
            { "Изкуство", "Art" },
            { "ИТ", "IT" },
            { "Информационни технологии", "IT" },
            { "Социални мрежи", "Social Media" },
            { "Околна среда", "Environment" },
            { "Екология", "Environment" },
            { "Образование", "Education" },
            { "Обучение", "Education" },
            { "Здравеопазване", "Health" },
            { "Здраве", "Health" },
            { "Социални", "Social" },
            { "Социално", "Social" },
            { "Социални дейности", "Social" },
            { "Защита на животните", "Animal Welfare" },
            { "Грижа за животните", "Animal Welfare" },
            { "Култура", "Culture" },
            { "Спорт", "Sports" },
            { "Друго", "Other" },

            // Regions
            { "София", "Sofia" },
            { "Пловдив", "Plovdiv" },
            { "Варна", "Varna" },
            { "Бургас", "Burgas" },
            { "Русе", "Ruse" },
            { "Стара Загора", "Stara Zagora" },
            { "Плевен", "Pleven" },
            { "Сливен", "Sliven" },
            { "Добрич", "Dobrich" },
            { "Шумен", "Shumen" },
            { "Перник", "Pernik" },
            { "Хасково", "Haskovo" },
            { "Благоевград", "Blagoevgrad" },
            { "Велико Търново", "Veliko Tarnovo" },
            { "Пазарджик", "Pazardzhik" },
            { "Враца", "Vratsa" },
            { "Габрово", "Gabrovo" },
            { "Асеновград", "Asenovgrad" },
            { "Казанлък", "Kazanlak" },
            { "Кърджали", "Kardzhali" },
            { "Кюстендил", "Kyustendil" },
            { "Монтана", "Montana" },
            { "Ловеч", "Lovech" },
            { "Търговище", "Targovishte" },
            { "Разград", "Razgrad" },
            { "Силистра", "Silistra" },
            { "Смолян", "Smolyan" },
            { "Ямбол", "Yambol" }
        };

        public InitiativeService(
            ApplicationDbContext context, 
            IEmailService emailService,
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Initiative>> GetAllInitiativesAsync(string? searchString = null, string? category = null, string? region = null)
        {
            /* Auto-finish logic removed from here to prevent write-back errors on GET requests */
            
            var initiatives = _context.Initiatives
                .Include(i => i.Organizer)
                .Include(i => i.Enrolments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var translatedSearch = BulgarianToEnglishMapping.TryGetValue(searchString, out var eng) ? eng : searchString;
                
                initiatives = initiatives.Where(s => s.Title!.Contains(searchString) 
                                               || s.Location!.Contains(searchString)
                                               || s.Description!.Contains(searchString)
                                               || (translatedSearch != searchString && (
                                                   s.Title!.Contains(translatedSearch!)
                                                   || s.Location!.Contains(translatedSearch!)
                                                   || s.Description!.Contains(translatedSearch!)
                                                   || s.Category!.Contains(translatedSearch!)
                                                   || s.Region!.Contains(translatedSearch!)
                                               )));
            }

            if (!string.IsNullOrEmpty(category))
            {
                var engCat = BulgarianToEnglishMapping.TryGetValue(category, out var eng) ? eng : category;
                initiatives = initiatives.Where(i => i.Category == engCat || i.Category == category);
            }

            if (!string.IsNullOrEmpty(region))
            {
                var engReg = BulgarianToEnglishMapping.TryGetValue(region, out var eng) ? eng : region;
                initiatives = initiatives.Where(i => i.Region == engReg || i.Region == region);
            }

            return await initiatives.OrderByDescending(i => i.DateAndTime).ToListAsync();
        }

        public async Task<IEnumerable<Initiative>> GetActiveInitiativesWithLocationAsync()
        {
            return await _context.Initiatives
                .Include(i => i.Enrolments)
                .Where(i => i.Status == MissionStatus.Active && i.Latitude != null && i.Longitude != null)
                .ToListAsync();
        }

        public async Task<Initiative?> GetInitiativeByIdAsync(int id)
        {
            return await _context.Initiatives
                .Include(i => i.Organizer)
                .Include(i => i.Tasks!)
                .Include(i => i.Enrolments!).ThenInclude(e => e.Volunteer)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Initiative> CreateInitiativeAsync(Initiative initiative, string organizerId)
        {
            initiative.OrganizerId = organizerId;
            initiative.Status = MissionStatus.Active;
            _context.Add(initiative);
            await _context.SaveChangesAsync();

            // Notify volunteers in the same location
            if (!string.IsNullOrEmpty(initiative.Location))
            {
                var volunteersInLocation = await _userManager.GetUsersInRoleAsync("Volunteer");
                var relevantVolunteers = volunteersInLocation
                    .Where(v => v.Location != null && v.Location.Contains(initiative.Location, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var volunteer in relevantVolunteers)
                {
                    await _emailService.SendNewInitiativeNotificationAsync(
                        volunteer.Email!,
                        volunteer.FullName!,
                        initiative.Title!,
                        initiative.Location);
                }
            }

            return initiative;
        }

        public async Task<IEnumerable<Initiative>> GetInitiativesByOrganizerAsync(string organizerId)
        {
            return await _context.Initiatives
                .Include(i => i.Enrolments)
                .Where(i => i.OrganizerId == organizerId)
                .ToListAsync();
        }

        public async Task<bool> DeleteInitiativeAsync(int id, string userId, bool isAdmin)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return false;

            if (initiative.OrganizerId != userId && !isAdmin)
            {
                return false;
            }

            _context.Initiatives.Remove(initiative);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FinishInitiativeAsync(int id, string userId, bool isAdmin)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return false;

            if (initiative.OrganizerId != userId && !isAdmin)
            {
                return false;
            }

            initiative.Status = MissionStatus.Finished;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleFilledStatusAsync(int id, string userId, bool isAdmin)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return false;

            if (initiative.OrganizerId != userId && !isAdmin)
            {
                return false;
            }

            if (initiative.Status == MissionStatus.Active)
                initiative.Status = MissionStatus.Filled;
            else if (initiative.Status == MissionStatus.Filled)
                initiative.Status = MissionStatus.Active;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
