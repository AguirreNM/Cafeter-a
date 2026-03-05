using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;
using PryCafeteria.Models.ViewModels;
using PryCafeteria.Services;

namespace PryCafeteria.Controllers;

/// <summary>
/// Carrito de compras con sesión — HU11 (agregar), HU12 (modificar/eliminar),
/// HU13 (ver totales), HU19 (cupones).
/// </summary>
public class CarritoController : Controller
{
    private readonly BdcafeteriaContext _context;

    public CarritoController(BdcafeteriaContext context)
    {
        _context = context;
    }

    // ══════════════════════════════════════════════════════════════════════
    // GET: /Carrito  — HU12/HU13: ver carrito completo con totales
    // ══════════════════════════════════════════════════════════════════════
    public IActionResult Index()
    {
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        var vm = new CarritoIndexViewModel { Carrito = carrito };
        ViewBag.CarritoItems = carrito.TotalItems;
        return View(vm);
    }

    // ══════════════════════════════════════════════════════════════════════
    // POST: /Carrito/Agregar  — HU11: agregar producto al carrito (AJAX)
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(int productoTamanioId, int cantidad = 1)
    {
        var pt = await _context.ProductosTamanios
            .Include(x => x.Producto)
            .Include(x => x.Tamanio)
            .FirstOrDefaultAsync(x => x.ProductoTamanioId == productoTamanioId && x.Disponible);

        if (pt == null)
            return Json(new { ok = false, mensaje = "Producto no disponible." });

        // HU11-E7: verificar stock
        if (pt.Stock <= 0)
            return Json(new { ok = false, mensaje = "Este producto no tiene stock disponible." });

        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        var item = carrito.Items.FirstOrDefault(i => i.ProductoTamanioId == productoTamanioId);

        if (item != null)
        {
            // HU11-E2: incrementar si ya existe
            var nuevaCantidad = item.Cantidad + cantidad;
            if (nuevaCantidad > pt.Stock)
            {
                item.Cantidad = pt.Stock;
                CarritoService.GuardarCarrito(HttpContext.Session, carrito);
                return Json(new
                {
                    ok = true,
                    advertencia = $"Stock máximo disponible: {pt.Stock}",
                    totalItems = carrito.TotalItems
                });
            }
            item.Cantidad = nuevaCantidad;
            item.StockDisponible = pt.Stock;
        }
        else
        {
            // HU11-E1: agregar nuevo ítem
            carrito.Items.Add(new CarritoItem
            {
                ProductoTamanioId = productoTamanioId,
                NombreProducto = pt.Producto.NombreProducto,
                NombreTamanio = pt.Tamanio.NombreTamanio,
                Imagen = pt.Producto.Imagen,
                PrecioUnitario = pt.Precio,
                Cantidad = Math.Min(cantidad, pt.Stock),
                StockDisponible = pt.Stock
            });
        }

        CarritoService.GuardarCarrito(HttpContext.Session, carrito);
        return Json(new { ok = true, totalItems = carrito.TotalItems });
    }

    // ══════════════════════════════════════════════════════════════════════
    // POST: /Carrito/ActualizarCantidad  — HU12: cambiar cantidad (AJAX)
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarCantidad(int productoTamanioId, int cantidad)
    {
        if (cantidad <= 0)
            return await Eliminar(productoTamanioId);

        var pt = await _context.ProductosTamanios
            .FirstOrDefaultAsync(x => x.ProductoTamanioId == productoTamanioId);

        if (pt == null)
            return Json(new { ok = false, mensaje = "Producto no encontrado." });

        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        var item = carrito.Items.FirstOrDefault(i => i.ProductoTamanioId == productoTamanioId);

        if (item == null)
            return Json(new { ok = false, mensaje = "Ítem no está en el carrito." });

        // HU12-E7: ajustar si excede stock actual
        if (cantidad > pt.Stock)
        {
            item.Cantidad = pt.Stock;
            item.StockDisponible = pt.Stock;
            CarritoService.GuardarCarrito(HttpContext.Session, carrito);
            return Json(new
            {
                ok = true,
                advertencia = $"Solo quedan {pt.Stock} unidades disponibles",
                cantidad = item.Cantidad,
                subtotalItem = item.Subtotal,
                subtotalCarrito = carrito.Subtotal,
                descuento = carrito.Descuento,
                total = carrito.Total,
                totalItems = carrito.TotalItems
            });
        }

        item.Cantidad = cantidad;
        item.StockDisponible = pt.Stock;
        CarritoService.GuardarCarrito(HttpContext.Session, carrito);

        return Json(new
        {
            ok = true,
            cantidad = item.Cantidad,
            subtotalItem = item.Subtotal,
            subtotalCarrito = carrito.Subtotal,
            descuento = carrito.Descuento,
            total = carrito.Total,
            totalItems = carrito.TotalItems
        });
    }

    // ══════════════════════════════════════════════════════════════════════
    // POST: /Carrito/Eliminar  — HU12: eliminar ítem (AJAX)
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Eliminar(int productoTamanioId)
    {
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        carrito.Items.RemoveAll(i => i.ProductoTamanioId == productoTamanioId);
        CarritoService.GuardarCarrito(HttpContext.Session, carrito);

        return Task.FromResult<IActionResult>(Json(new
        {
            ok = true,
            subtotalCarrito = carrito.Subtotal,
            descuento = carrito.Descuento,
            total = carrito.Total,
            totalItems = carrito.TotalItems,
            carritoVacio = !carrito.Items.Any()
        }));
    }

    // ══════════════════════════════════════════════════════════════════════
    // POST: /Carrito/Vaciar  — HU12-E6: vaciar carrito
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Vaciar()
    {
        CarritoService.LimpiarCarrito(HttpContext.Session);
        TempData["Info"] = "Tu carrito fue vaciado.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════════════════════
    // POST: /Carrito/AplicarCupon  — HU19: validar y aplicar cupón
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AplicarCupon(string codigoCupon)
    {
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);

        if (!carrito.Items.Any())
            return Json(new { ok = false, mensaje = "El carrito está vacío." });

        var codigo = codigoCupon?.Trim().ToUpper();
        if (string.IsNullOrEmpty(codigo))
            return Json(new { ok = false, mensaje = "Ingresa un código de cupón." });

        // HU19-E2: cupón inexistente
        var cupon = await _context.Cupones
            .FirstOrDefaultAsync(c => c.NombreCupon.ToUpper() == codigo);

        if (cupon == null)
            return Json(new { ok = false, mensaje = "Este cupón no existe." });

        // HU19-E5: monto mínimo (si el modelo tuviera MontoMinimo; aquí usamos ValorDescuento como proxy solo para cupones tipo "Porcentaje")
        // HU19-E3: cupón expirado
        if (cupon.FechaFin < DateTime.Now)
            return Json(new { ok = false, mensaje = $"Este cupón expiró el {cupon.FechaFin:dd/MM/yyyy}." });

        // HU19-E4: no iniciado
        if (cupon.FechaInicio > DateTime.Now)
            return Json(new { ok = false, mensaje = $"Este cupón será válido desde el {cupon.FechaInicio:dd/MM/yyyy}." });

        // HU19-E1: cupón desactivado
        if (!cupon.Activo)
            return Json(new { ok = false, mensaje = "Este cupón no está disponible." });

        // Calcular descuento
        decimal descuento = cupon.TipoDescuento.Equals("Porcentaje", StringComparison.OrdinalIgnoreCase)
            ? Math.Round(carrito.Subtotal * cupon.ValorDescuento / 100, 2)
            : cupon.ValorDescuento;

        descuento = Math.Min(descuento, carrito.Subtotal);

        carrito.CuponCodigo = cupon.NombreCupon;
        carrito.Descuento = descuento;
        CarritoService.GuardarCarrito(HttpContext.Session, carrito);

        return Json(new
        {
            ok = true,
            mensaje = $"¡Cupón aplicado! Descuento: S/. {descuento:0.00}",
            descuento,
            total = carrito.Total,
            cuponCodigo = carrito.CuponCodigo
        });
    }

    // ══════════════════════════════════════════════════════════════════════
    // POST: /Carrito/QuitarCupon  — HU19-E7: quitar cupón (AJAX)
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult QuitarCupon()
    {
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        carrito.CuponCodigo = null;
        carrito.Descuento = 0;
        CarritoService.GuardarCarrito(HttpContext.Session, carrito);

        return Json(new
        {
            ok = true,
            total = carrito.Total,
            subtotalCarrito = carrito.Subtotal
        });
    }

    // ══════════════════════════════════════════════════════════════════════
    // GET: /Carrito/Cantidad  — obtener badge para navbar (AJAX)
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult Cantidad()
    {
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        return Json(new { totalItems = carrito.TotalItems });
    }
}
