using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using onlineshop.Models;
using onlineshop.Models.Db;

namespace onlineshop.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly OnlineShopContext _context;

    public HomeController(ILogger<HomeController> logger, OnlineShopContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var banners = _context.Banners.ToList();
        ViewData["banners"] = banners;
        //-------------------new products-------------
        var newProducts = _context.Products.OrderByDescending(x => x.Id).Take(8).ToList();
        ViewData["newProducts"] = newProducts;

        //-------------------best selling-------------
        var bestSellingProducts = _context.BestSellingFinals.ToList();
        ViewData["bestSellingProducts"] = bestSellingProducts;

        //--------------------------------------------
        return View();
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
