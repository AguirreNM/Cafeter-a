using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PryCafeteria.Models;
using PryCafeteria.Models.ViewModels;

namespace PryCafeteria.Controllers
{
    public class CuentasController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CuentasController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Cuentas/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: Cuentas/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user!);
                    if (roles.Contains("Admin"))
                        return RedirectToAction("Index", "Dashboard");

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            }

            return View(model);
        }

        // GET: Cuentas/Registro - redirige al panel de registro en Login
        [HttpGet]
        public IActionResult Registro()
        {
            ViewBag.MostrarRegistro = true;
            return View("Login", new LoginViewModel());
        }

        // POST: Cuentas/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ViewBag.MostrarRegistro = true;
                    ViewBag.ErroresRegistro = new List<string> { "Este correo ya está registrado." };
                    ViewBag.RegistroNombre = model.Nombre;
                    ViewBag.RegistroApellido = model.Apellido;
                    ViewBag.RegistroEmail = model.Email;
                    return View("Login", new LoginViewModel());
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    FechaRegistro = DateTime.Now,
                    // HU05 - E8: el email se confirmará en flujo futuro por correo
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Cliente");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Si falla, volver al panel de registro con los errores
                ViewBag.MostrarRegistro = true;
                ViewBag.ErroresRegistro = result.Errors.Select(e => e.Description).ToList();
                ViewBag.RegistroNombre = model.Nombre;
                ViewBag.RegistroApellido = model.Apellido;
                ViewBag.RegistroEmail = model.Email;
            }
            else
            {
                // Errores de validación del modelo
                ViewBag.MostrarRegistro = true;
                ViewBag.ErroresRegistro = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                ViewBag.RegistroNombre = model.Nombre;
                ViewBag.RegistroApellido = model.Apellido;
                ViewBag.RegistroEmail = model.Email;
            }

            return View("Login", new LoginViewModel());
        }

        // POST: Cuentas/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
