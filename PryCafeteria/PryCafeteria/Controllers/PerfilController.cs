using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PryCafeteria.Models;
using PryCafeteria.Services;

namespace PryCafeteria.Controllers;

[Authorize]
public class PerfilController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public PerfilController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /Perfil
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var vm = new PerfilViewModel
        {
            Nombre = user.Nombre ?? "",
            Apellido = user.Apellido ?? "",
            Email = user.Email ?? ""
        };

        if (!User.IsInRole("Admin"))
        {
            var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
            ViewBag.CarritoItems = carrito.TotalItems;
        }
        return View(vm);
    }

    // POST: /Perfil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PerfilViewModel vm)
    {
        ModelState.Remove("NuevaPassword");
        ModelState.Remove("ConfirmarPassword");
        ModelState.Remove("PasswordActual");

        if (!ModelState.IsValid)
        {
            if (!User.IsInRole("Admin"))
                ViewBag.CarritoItems = CarritoService.ObtenerCarrito(HttpContext.Session).TotalItems;
            return View(vm);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.Nombre = vm.Nombre;
        user.Apellido = vm.Apellido;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Exito"] = "Perfil actualizado correctamente.";
        }
        else
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
        }

        if (!User.IsInRole("Admin"))
            ViewBag.CarritoItems = CarritoService.ObtenerCarrito(HttpContext.Session).TotalItems;
        return View(vm);
    }

    // POST: /Perfil/CambiarPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(PerfilViewModel vm)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (string.IsNullOrEmpty(vm.PasswordActual))
        {
            TempData["ErrorPassword"] = "Ingresa tu contraseña actual.";
            return RedirectToAction(nameof(Index));
        }
        if (string.IsNullOrEmpty(vm.NuevaPassword) || vm.NuevaPassword.Length < 6)
        {
            TempData["ErrorPassword"] = "La nueva contraseña debe tener al menos 6 caracteres.";
            return RedirectToAction(nameof(Index));
        }
        if (vm.NuevaPassword != vm.ConfirmarPassword)
        {
            TempData["ErrorPassword"] = "Las contraseñas no coinciden.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.ChangePasswordAsync(user,
            vm.PasswordActual, vm.NuevaPassword);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["ExitoPassword"] = "Contraseña cambiada exitosamente.";
        }
        else
        {
            TempData["ErrorPassword"] = result.Errors.FirstOrDefault()?.Description
                ?? "No se pudo cambiar la contraseña.";
        }
        return RedirectToAction(nameof(Index));
    }
}

// ── ViewModel ────────────────────────────────────────────────────────────────
public class PerfilViewModel
{
    [Required, StringLength(100), Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";

    [Required, StringLength(100), Display(Name = "Apellido")]
    public string Apellido { get; set; } = "";

    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = "";

    // Cambio de contraseña (opcionales)
    [Display(Name = "Contraseña actual")]
    public string? PasswordActual { get; set; }

    [Display(Name = "Nueva contraseña")]
    [StringLength(100, MinimumLength = 6)]
    public string? NuevaPassword { get; set; }

    [Display(Name = "Confirmar nueva contraseña")]
    [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden.")]
    public string? ConfirmarPassword { get; set; }
}
