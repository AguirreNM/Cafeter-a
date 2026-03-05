using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PryCafeteria.Models;

namespace PryCafeteria.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Dashboard");

            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Catalogo");

            return View();
        }
    }
}
