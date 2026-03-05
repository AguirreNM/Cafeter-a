using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;
using PryCafeteria.Services;

namespace PryCafeteria.Controllers;

/// <summary>
/// Gestión de direcciones de entrega del cliente.
/// Solo el propietario puede ver/crear/editar/eliminar sus propias direcciones.
/// </summary>
[Authorize]
public class DireccionesController : Controller
{
    private readonly BdcafeteriaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DireccionesController(BdcafeteriaContext context,
                                  UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Direcciones — lista de direcciones del usuario
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var dirs = await _context.DireccionesEntregas
            .Where(d => d.UsuarioId == userId)
            .OrderBy(d => d.NombreDireccion)
            .ToListAsync();

        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        ViewBag.CarritoItems = carrito.TotalItems;
        return View(dirs);
    }

    // GET: /Direcciones/Create
    public IActionResult Create()
    {
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        ViewBag.CarritoItems = carrito.TotalItems;
        return View(new DireccionesEntrega());
    }

    // POST: /Direcciones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("NombreDireccion,Calle,Numero,Distrito,CodigoPostal,Referencias")]
        DireccionesEntrega dir)
    {
        ModelState.Remove("UsuarioId");
        ModelState.Remove("Usuario");
        ModelState.Remove("Pedidos");

        if (ModelState.IsValid)
        {
            dir.UsuarioId = _userManager.GetUserId(User)!;
            _context.DireccionesEntregas.Add(dir);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Dirección guardada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        ViewBag.CarritoItems = carrito.TotalItems;
        return View(dir);
    }

    // GET: /Direcciones/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User);
        var dir = await _context.DireccionesEntregas
            .FirstOrDefaultAsync(d => d.DireccionId == id && d.UsuarioId == userId);
        if (dir == null) return NotFound();

        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        ViewBag.CarritoItems = carrito.TotalItems;
        return View(dir);
    }

    // POST: /Direcciones/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("DireccionId,NombreDireccion,Calle,Numero,Distrito,CodigoPostal,Referencias")]
        DireccionesEntrega dir)
    {
        if (id != dir.DireccionId) return NotFound();

        var userId = _userManager.GetUserId(User);
        var existente = await _context.DireccionesEntregas
            .FirstOrDefaultAsync(d => d.DireccionId == id && d.UsuarioId == userId);
        if (existente == null) return NotFound();

        ModelState.Remove("UsuarioId");
        ModelState.Remove("Usuario");
        ModelState.Remove("Pedidos");

        if (ModelState.IsValid)
        {
            existente.NombreDireccion = dir.NombreDireccion;
            existente.Calle = dir.Calle;
            existente.Numero = dir.Numero;
            existente.Distrito = dir.Distrito;
            existente.CodigoPostal = dir.CodigoPostal;
            existente.Referencias = dir.Referencias;
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Dirección actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        ViewBag.CarritoItems = carrito.TotalItems;
        return View(dir);
    }

    // POST: /Direcciones/Eliminar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var userId = _userManager.GetUserId(User);
        var dir = await _context.DireccionesEntregas
            .Include(d => d.Pedidos)
            .FirstOrDefaultAsync(d => d.DireccionId == id && d.UsuarioId == userId);

        if (dir == null) return NotFound();

        if (dir.Pedidos.Any())
        {
            TempData["Error"] = "Esta dirección tiene pedidos asociados y no puede eliminarse.";
            return RedirectToAction(nameof(Index));
        }

        _context.DireccionesEntregas.Remove(dir);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Dirección eliminada.";
        return RedirectToAction(nameof(Index));
    }
}
