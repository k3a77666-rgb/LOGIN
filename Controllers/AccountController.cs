using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LOGIN.Models;
using LOGIN.Data;

namespace LOGIN.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================
        // GET: LOGIN
        // ============================================
        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // ============================================
        // POST: LOGIN
        // ============================================
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

                if (usuario != null)
                {
                    HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
                    HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "");
                    HttpContext.Session.SetString("UsuarioEmail", usuario.Email ?? "");

                    // 🔥 MENSAJE DE BIENVENIDA
                    TempData["Mensaje"] = $"¡Bienvenido {usuario.Nombre}!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email o contraseña incorrectos");
                }
            }
            return View(model);
        }

        // ============================================
        // GET: REGISTER
        // ============================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ============================================
        // POST: REGISTER
        // ============================================
        [HttpPost]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el email ya existe
                var existe = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
                if (existe)
                {
                    ModelState.AddModelError("Email", "Este email ya está registrado");
                    return View(usuario);
                }

                // Asignar fecha de registro (UTC) y guardar
                usuario.FechaRegistro = DateTime.UtcNow;
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // 🔥 MENSAJE DE ÉXITO
                TempData["Mensaje"] = "Registro exitoso. ¡Inicia sesión!";
                return RedirectToAction("Login");
            }
            return View(usuario);
        }

        // ============================================
        // LOGOUT
        // ============================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Mensaje"] = "Sesión cerrada correctamente.";
            return RedirectToAction("Login");
        }
    }
}