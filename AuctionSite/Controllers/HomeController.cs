using AuctionSite.Data;
using AuctionSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using AuctionSite.Resources;
using System.Globalization;

namespace AuctionSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<SharedResources> _sharedLocalizer;

        public HomeController(ApplicationDbContext context, IStringLocalizer<SharedResources> sharedLocalizer)
        {
            _context = context;
            _sharedLocalizer = sharedLocalizer;
        }

        public async Task<IActionResult> Index()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            DateTime nowUtc = DateTime.UtcNow; // Karþýlaþtýrmalar için UTC kullanýyoruz

            var activeAuctions = await _context.AuctionItems
                .Include(a => a.Seller)
                .Where(item => item.StartTime <= nowUtc && item.EndTime > nowUtc)
                .OrderBy(item => item.EndTime)
                .ToListAsync();

            var upcomingAuctions = await _context.AuctionItems
                .Include(a => a.Seller)
                .Where(item => item.StartTime > nowUtc)
                .OrderBy(item => item.StartTime)
                .ToListAsync();

            // GetString metodunun düzeltilmesi
            ViewData["Title"] = SharedResources.ResourceManager.GetString("ActiveAuctions") ?? "";

            ViewBag.ActiveAuctions = activeAuctions;
            ViewBag.UpcomingAuctions = upcomingAuctions;

            return View();
        }

        public IActionResult Privacy()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            // GetString metodunun düzeltilmesi
            ViewData["Title"] = SharedResources.ResourceManager.GetString("Privacy") ?? "";
            return View();
        }

        // Yeni HowItWorks metodu
        public IActionResult HowItWorks()
        {
            // GetString metodunun düzeltilmesi
            ViewData["Title"] = SharedResources.ResourceManager.GetString("HowItWorksTitle") ?? "Nasýl Çalýþýr?";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            // GetString metodunun düzeltilmesi
            ViewData["Title"] = SharedResources.ResourceManager.GetString("Error") ?? "";
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}