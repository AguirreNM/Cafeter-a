using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;
using PryCafeteria.Models.ViewModels;
using PryCafeteria.Services;

namespace PryCafeteria.Controllers;

/// <summary>
/// Gestión de pedidos.
/// Cliente: Checkout (HU14), Mis Pedidos (HU15), Cancelar.
/// Admin:   Listar todos (HU16), Cambiar estado, Ver detalle.
/// </summary>
[Authorize]
public class PedidosController : Controller
{
    private readonly BdcafeteriaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PedidosController(BdcafeteriaContext context,
                              UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ══════════════════════════════════════════════════════════════════════
    // ── ZONA CLIENTE ──────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════

    // GET: /Pedidos/Checkout  — HU14: formulario de checkout
    public async Task<IActionResult> Checkout()
    {
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        if (!carrito.Items.Any())
        {
            TempData["Info"] = "Tu carrito está vacío.";
            return RedirectToAction("Index", "Carrito");
        }

        var userId = _userManager.GetUserId(User);

        var vm = new CheckoutViewModel
        {
            Carrito = carrito,
            MetodosPago = await _context.MetodosPagos
                .Where(m => m.Disponible).ToListAsync(),
            Direcciones = await _context.DireccionesEntregas
                .Where(d => d.UsuarioId == userId).ToListAsync()
        };

        ViewBag.CarritoItems = carrito.TotalItems;
        return View(vm);
    }

    // POST: /Pedidos/Checkout  — HU14: confirmar pedido
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel vm)
    {
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        if (!carrito.Items.Any())
        {
            TempData["Info"] = "Tu carrito está vacío.";
            return RedirectToAction("Index", "Carrito");
        }

        var userId = _userManager.GetUserId(User)!;

        // Validar que si es Delivery haya dirección
        if (vm.TipoEntrega == PryCafeteria.Models.TipoEntrega.Delivery && !vm.DireccionId.HasValue)
        {
            // Intentar crear dirección nueva si se llenaron los campos
            if (vm.NuevaDireccion != null
                && !string.IsNullOrWhiteSpace(vm.NuevaDireccion.Calle)
                && !string.IsNullOrWhiteSpace(vm.NuevaDireccion.Distrito))
            {
                var nuevaDir = new DireccionesEntrega
                {
                    UsuarioId = userId,
                    NombreDireccion = vm.NuevaDireccion.NombreDireccion ?? "Mi dirección",
                    Calle = vm.NuevaDireccion.Calle!,
                    Numero = vm.NuevaDireccion.Numero ?? "S/N",
                    Distrito = vm.NuevaDireccion.Distrito!,
                    CodigoPostal = vm.NuevaDireccion.CodigoPostal ?? "00000",
                    Referencias = vm.NuevaDireccion.Referencias
                };
                _context.DireccionesEntregas.Add(nuevaDir);
                await _context.SaveChangesAsync();
                vm.DireccionId = nuevaDir.DireccionId;
            }
            else
            {
                ModelState.AddModelError("DireccionId",
                    "Para delivery debes seleccionar o ingresar una dirección.");
            }
        }

        // Limpiar validaciones de NuevaDireccion si aplica
        foreach (var key in ModelState.Keys
            .Where(k => k.StartsWith("NuevaDireccion")).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            vm.Carrito = carrito;
            vm.MetodosPago = await _context.MetodosPagos
                .Where(m => m.Disponible).ToListAsync();
            vm.Direcciones = await _context.DireccionesEntregas
                .Where(d => d.UsuarioId == userId).ToListAsync();
            ViewBag.CarritoItems = carrito.TotalItems;
            return View(vm);
        }

        // ── HU14-E7: validar stock antes de confirmar (transacción) ────────
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var sinStock = new List<string>();

            foreach (var item in carrito.Items)
            {
                var pt = await _context.ProductosTamanios
                    .FirstOrDefaultAsync(x => x.ProductoTamanioId == item.ProductoTamanioId);

                if (pt == null || pt.Stock < item.Cantidad)
                    sinStock.Add($"{item.NombreProducto} {item.NombreTamanio}" +
                        $" (disponible: {pt?.Stock ?? 0}, pedido: {item.Cantidad})");
            }

            if (sinStock.Any())
            {
                TempData["Error"] = "Stock insuficiente: " + string.Join(", ", sinStock);
                vm.Carrito = carrito;
                vm.MetodosPago = await _context.MetodosPagos
                    .Where(m => m.Disponible).ToListAsync();
                vm.Direcciones = await _context.DireccionesEntregas
                    .Where(d => d.UsuarioId == userId).ToListAsync();
                ViewBag.CarritoItems = carrito.TotalItems;
                return View(vm);
            }

            // Crear el pedido
            var pedido = new Pedido
            {
                UsuarioId = userId,
                FechaPedido = DateTime.Now,
                MetodoPagoId = vm.MetodoPagoId,
                TipoEntrega = vm.TipoEntrega,
                DireccionId = vm.TipoEntrega == PryCafeteria.Models.TipoEntrega.Delivery
                    ? vm.DireccionId : null,
                Estado = EstadoPedido.Pendiente,
                Descuento = carrito.Descuento,
                Total = carrito.Total
            };
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // Crear detalles y descontar stock
            foreach (var item in carrito.Items)
            {
                var pt = await _context.ProductosTamanios
                    .FirstAsync(x => x.ProductoTamanioId == item.ProductoTamanioId);

                _context.DetallePedidos.Add(new DetallePedido
                {
                    PedidoId = pedido.PedidoId,
                    ProductoTamanioId = item.ProductoTamanioId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    NombreProductoSnapshot = item.NombreProducto,
                    NombreTamanioSnapshot = item.NombreTamanio
                });

                // HU10: descontar stock
                pt.Stock -= item.Cantidad;

                // Registrar movimiento
                _context.StockMovimientos.Add(new StockMovimiento
                {
                    ProductoTamanioId = item.ProductoTamanioId,
                    Tipo = "Venta",
                    Cantidad = -item.Cantidad,
                    StockResultante = pt.Stock,
                    Referencia = $"Pedido #{pedido.PedidoId}",
                    UsuarioId = null,
                    Fecha = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Vaciar carrito tras confirmar
            CarritoService.LimpiarCarrito(HttpContext.Session);

            return RedirectToAction(nameof(Confirmacion), new { id = pedido.PedidoId });
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Ocurrió un error al procesar el pedido. Intenta de nuevo.";
            vm.Carrito = carrito;
            vm.MetodosPago = await _context.MetodosPagos
                .Where(m => m.Disponible).ToListAsync();
            vm.Direcciones = await _context.DireccionesEntregas
                .Where(d => d.UsuarioId == userId).ToListAsync();
            ViewBag.CarritoItems = carrito.TotalItems;
            return View(vm);
        }
    }

    // GET: /Pedidos/Confirmacion/5  — HU14-E8: página de confirmación
    public async Task<IActionResult> Confirmacion(int id)
    {
        var userId = _userManager.GetUserId(User);
        var pedido = await _context.Pedidos
            .Include(p => p.DetallePedidos)
                .ThenInclude(d => d.ProductoTamanio)
                    .ThenInclude(pt => pt.Producto)
            .Include(p => p.MetodoPago)
            .Include(p => p.Direccion)
            .FirstOrDefaultAsync(p => p.PedidoId == id && p.UsuarioId == userId);

        if (pedido == null) return NotFound();
        ViewBag.CarritoItems = 0;
        return View(pedido);
    }

    // GET: /Pedidos/MisPedidos  — HU15-E1: listar pedidos del cliente
    public async Task<IActionResult> MisPedidos(string? estado)
    {
        var userId = _userManager.GetUserId(User);
        var query = _context.Pedidos
            .Where(p => p.UsuarioId == userId)
            .Include(p => p.DetallePedidos)
            .Include(p => p.MetodoPago)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(p => p.Estado == estado);

        var pedidos = await query
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();

        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        ViewBag.CarritoItems = carrito.TotalItems;
        ViewBag.FiltroEstado = estado;
        return View(pedidos);
    }

    // GET: /Pedidos/Detalle/5  — HU15-E3: detalle con timeline
    public async Task<IActionResult> Detalle(int id)
    {
        var userId = _userManager.GetUserId(User);
        var esAdmin = User.IsInRole("Admin");

        var pedido = await _context.Pedidos
            .Include(p => p.DetallePedidos)
                .ThenInclude(d => d.ProductoTamanio)
                    .ThenInclude(pt => pt.Producto)
            .Include(p => p.DetallePedidos)
                .ThenInclude(d => d.ProductoTamanio)
                    .ThenInclude(pt => pt.Tamanio)
            .Include(p => p.MetodoPago)
            .Include(p => p.Direccion)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.PedidoId == id
                && (esAdmin || p.UsuarioId == userId));

        if (pedido == null) return NotFound();

        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        ViewBag.CarritoItems = carrito.TotalItems;
        return View(pedido);
    }

    // POST: /Pedidos/Cancelar/5  — HU15-E6: cliente cancela pedido pendiente
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var userId = _userManager.GetUserId(User);
        var pedido = await _context.Pedidos
            .Include(p => p.DetallePedidos)
            .FirstOrDefaultAsync(p => p.PedidoId == id && p.UsuarioId == userId);

        if (pedido == null) return NotFound();

        // HU15-E7: solo se puede cancelar si está Pendiente
        if (pedido.Estado != EstadoPedido.Pendiente)
        {
            TempData["Error"] = "No se puede cancelar un pedido que ya está en proceso.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        pedido.Estado = EstadoPedido.Cancelado;

        // HU10-E3: devolver stock
        foreach (var det in pedido.DetallePedidos)
        {
            var pt = await _context.ProductosTamanios
                .FirstOrDefaultAsync(x => x.ProductoTamanioId == det.ProductoTamanioId);
            if (pt != null)
            {
                pt.Stock += det.Cantidad;
                _context.StockMovimientos.Add(new StockMovimiento
                {
                    ProductoTamanioId = det.ProductoTamanioId,
                    Tipo = "Devolucion",
                    Cantidad = det.Cantidad,
                    StockResultante = pt.Stock,
                    Referencia = $"Cancelación pedido #{id}",
                    UsuarioId = userId,
                    Fecha = DateTime.Now
                });
            }
        }

        await _context.SaveChangesAsync();
        TempData["Exito"] = "Pedido cancelado exitosamente.";
        return RedirectToAction(nameof(MisPedidos));
    }

    // ══════════════════════════════════════════════════════════════════════
    // ── ZONA ADMIN ────────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════

    // GET: /Pedidos  — HU16-E1: todos los pedidos
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(string? estado, string? busqueda, string? fecha)
    {
        var query = _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.MetodoPago)
            .Include(p => p.DetallePedidos)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(p => p.Estado == estado);

        if (!string.IsNullOrEmpty(busqueda))
        {
            if (int.TryParse(busqueda.TrimStart('#'), out var numPedido))
                query = query.Where(p => p.PedidoId == numPedido);
            else
                query = query.Where(p =>
                    p.Usuario.Nombre.Contains(busqueda) ||
                    (p.Usuario.Email != null && p.Usuario.Email.Contains(busqueda)));
        }

        if (fecha == "hoy")
            query = query.Where(p => p.FechaPedido.Date == DateTime.Today);
        else if (fecha == "semana")
            query = query.Where(p => p.FechaPedido >= DateTime.Today.AddDays(-7));

        var pedidos = await query
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();

        ViewBag.FiltroEstado = estado;
        ViewBag.FiltroBusqueda = busqueda;
        ViewBag.FiltroFecha = fecha;
        return View(pedidos);
    }

    // GET: /Pedidos/ExportarExcel  — HU16-E14: exportar pedidos filtrados
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportarExcel(string? estado, string? fecha)
    {
        var query = _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.MetodoPago)
            .Include(p => p.DetallePedidos)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(p => p.Estado == estado);

        if (fecha == "hoy")
            query = query.Where(p => p.FechaPedido.Date == DateTime.Today);
        else if (fecha == "semana")
            query = query.Where(p => p.FechaPedido >= DateTime.Today.AddDays(-7));
        else if (fecha == "mes")
            query = query.Where(p => p.FechaPedido >= DateTime.Today.AddDays(-30));

        var pedidos = await query.OrderByDescending(p => p.FechaPedido).ToListAsync();
        var bytes = PryCafeteria.Services.ExcelService.ExportarPedidos(pedidos);
        var sufijo = string.IsNullOrEmpty(estado) ? "Todos" : estado;
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Pedidos_{sufijo}_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // POST: /Pedidos/CambiarEstado  — HU16-E7: cambiar estado del pedido
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
    {
        if (!EstadoPedido.Todos.Contains(nuevoEstado))
            return BadRequest("Estado inválido.");

        var pedido = await _context.Pedidos
            .Include(p => p.DetallePedidos)
            .FirstOrDefaultAsync(p => p.PedidoId == id);

        if (pedido == null) return NotFound();

        var adminId = _userManager.GetUserId(User);

        // Si se cancela desde admin, devolver stock
        if (nuevoEstado == EstadoPedido.Cancelado
            && pedido.Estado != EstadoPedido.Cancelado)
        {
            foreach (var det in pedido.DetallePedidos)
            {
                var pt = await _context.ProductosTamanios
                    .FirstOrDefaultAsync(x => x.ProductoTamanioId == det.ProductoTamanioId);
                if (pt != null)
                {
                    pt.Stock += det.Cantidad;
                    _context.StockMovimientos.Add(new StockMovimiento
                    {
                        ProductoTamanioId = det.ProductoTamanioId,
                        Tipo = "Devolucion",
                        Cantidad = det.Cantidad,
                        StockResultante = pt.Stock,
                        Referencia = $"Cancelación admin pedido #{id}",
                        UsuarioId = adminId,
                        Fecha = DateTime.Now
                    });
                }
            }
        }

        pedido.Estado = nuevoEstado;
        await _context.SaveChangesAsync();

        TempData["Exito"] = $"Pedido #{id} actualizado a \"{nuevoEstado}\".";
        return RedirectToAction(nameof(Index));
    }
}
