using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Leaderboard()
    {
        var volunteers = await _context.Users
            .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Volunteer")))
            .Select(u => new VolunteerStats
            {
                FullName = u.FullName ?? u.UserName,
                CompletedMissions = _context.Enrolments.Count(e => e.VolunteerId == u.Id && e.Status == EnrolmentStatus.Approved),
                TotalPoints = (_context.Enrolments.Count(e => e.VolunteerId == u.Id && e.Status == EnrolmentStatus.Approved) * 100) + (u.Rating * 10)
            })
            .OrderByDescending(v => v.TotalPoints)
            .Take(10)
            .ToListAsync();

        int rank = 1;
        foreach (var v in volunteers)
        {
            v.Rank = rank++;
            if (v.TotalPoints >= 1000) { v.Badge = "Legend"; v.BadgeColor = "bg-warning text-dark"; }
            else if (v.TotalPoints >= 500) { v.Badge = "Hero"; v.BadgeColor = "bg-primary"; }
            else if (v.TotalPoints >= 100) { v.Badge = "Rising Star"; v.BadgeColor = "bg-success"; }
            else { v.Badge = "Newbie"; v.BadgeColor = "bg-secondary"; }
        }

        var model = new LeaderboardViewModel { TopVolunteers = volunteers };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
