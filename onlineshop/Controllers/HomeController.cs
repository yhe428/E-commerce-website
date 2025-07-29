using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using onlineshop.Models;
using onlineshop.Models.Db;

namespace onlineshop.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly OnlineShopContext _context;
    private readonly IMemoryCache _cache;

    public HomeController(ILogger<HomeController> logger, OnlineShopContext context, IMemoryCache cache)
    {
        _logger = logger;
        _context = context;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Cache banners for 30 minutes
            var banners = await _cache.GetOrCreateAsync("banners", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await _context.Banners.ToListAsync();
            });
            ViewData["banners"] = banners;

            // Cache new products for 10 minutes
            var newProducts = await _cache.GetOrCreateAsync("newProducts", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _context.Products
                    .OrderByDescending(x => x.Id)
                    .Take(8)
                    .ToListAsync();
            });
            ViewData["newProducts"] = newProducts;

            // Cache best selling products for 15 minutes
            var bestSellingProducts = await _cache.GetOrCreateAsync("bestSellingProducts", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return await _context.BestSellingFinals.ToListAsync();
            });
            ViewData["bestSellingProducts"] = bestSellingProducts;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading home page");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
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
