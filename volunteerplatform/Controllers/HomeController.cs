using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;
using volunteerplatform.Services;

namespace volunteerplatform.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IStatisticsService _statisticsService;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, IStatisticsService statisticsService, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _statisticsService = statisticsService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null && User.IsInRole("Volunteer"))
        {
            var recommended = await _statisticsService.GetRecommendedInitiativesAsync(user.Id);
            ViewBag.Recommended = recommended;
        }

        var stats = await _statisticsService.GetHomeStatsAsync();
        ViewBag.Stats = stats;

        return View();
    }

    public async Task<IActionResult> Leaderboard()
    {
        var volunteers = await _statisticsService.GetLeaderboardAsync();
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
