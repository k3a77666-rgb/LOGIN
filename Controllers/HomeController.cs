using Microsoft.AspNetCore.Mvc;
using LOGIN.Data;
using LOGIN.Models;
using Microsoft.EntityFrameworkCore;

namespace LOGIN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Si no está logueado, redirigir al login
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.UsuarioNombre = usuarioNombre;
            ViewBag.UsuarioEmail = HttpContext.Session.GetString("UsuarioEmail");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Support()
        {
            return View();
        }
    }
}