using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using ProlabWeb.Models;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ProlabwebContext _context;

    public HomeController(ILogger<HomeController> logger, ProlabwebContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var dashboardData = new DashboardViewModel
        {
            PatientsCount = await _context.Patients.CountAsync(),
            DemandesCount = await _context.Entetedemandes.CountAsync(),
            ResultatsCount = await _context.Enteteresultats.CountAsync(),
            ValidationsCount = await _context.Enteteresultats
                .Where(e => e.Validationbiologiste == true)
                .CountAsync(),
            DerniersResultats = await _context.Enteteresultats
                .Include(e => e.CodesiteNavigation)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Patient)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Prescripteur)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Policeassurance)
                        .ThenInclude(pa => pa.CodeassuranceNavigation)
                .Include(e => e.IdanalyseNavigation)
                    .ThenInclude(a => a.Categorieanalyses)
                        .ThenInclude(ca => ca.Categorie)
                .Include(e => e.Technicien)
                .OrderByDescending(e => e.Date)
                .Take(10)
                .ToListAsync()
        };

        return View(dashboardData);
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
