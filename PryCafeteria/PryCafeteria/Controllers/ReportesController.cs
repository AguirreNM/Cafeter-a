using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;
using PryCafeteria.Services;

namespace PryCafeteria.Controllers;

/// <summary>
/// HU20-E6: Página de reportes con exportación a Excel.
/// Reportes: Ventas, Productos más vendidos, Clientes frecuentes,
///           Uso de cupones, Stock actual.
/// </summary>
[Authorize(Roles = "Admin")]
public class ReportesController : Controller
{
    private readonly BdcafeteriaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportesController(BdcafeteriaContext context,
                               UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Reportes — Página principal con métricas y links de descarga
    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var mesInicio = new DateTime(hoy.Year, hoy.Month, 1);

        // ── Resumen del mes ──────────────────────────────────────────────
        ViewBag.TotalPedidosMes = await _context.Pedidos
            .CountAsync(p => p.FechaPedido >= mesInicio);

        ViewBag.IngresosMes = await _context.Pedidos
            .Where(p => p.FechaPedido >= mesInicio && p.Estado == "Entregado")
            .SumAsync(p => (decimal?)p.Total) ?? 0;

        ViewBag.PedidosCancelados = await _context.Pedidos
            .CountAsync(p => p.FechaPedido >= mesInicio && p.Estado == "Cancelado");

        ViewBag.ClientesNuevosMes = _userManager.Users
            .Count(u => u.FechaRegistro >= mesInicio);

        // ── Top 5 productos del mes ──────────────────────────────────────
        ViewBag.TopProductos = await _context.DetallePedidos
            .Where(d => d.Pedido.FechaPedido >= mesInicio)
            .GroupBy(d => d.NombreProductoSnapshot)
            .Select(g => new { Nombre = g.Key ?? "—", Cantidad = g.Sum(x => x.Cantidad) })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToListAsync();

        // ── Cupones más usados ───────────────────────────────────────────
        ViewBag.TotalCupones = await _context.Cupones.CountAsync();
        ViewBag.CuponesActivos = await _context.Cupones.CountAsync(c => c.Activo);

        return View();
    }

    // GET: /Reportes/ExportarVentas?periodo=mes|semana|hoy
    public async Task<IActionResult> ExportarVentas(string periodo = "mes")
    {
        var desde = periodo switch
        {
            "hoy"    => DateTime.Today,
            "semana" => DateTime.Today.AddDays(-7),
            _        => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
        };

        var pedidos = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.MetodoPago)
            .Include(p => p.DetallePedidos)
            .Where(p => p.FechaPedido >= desde)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();

        var bytes = ExcelService.ExportarPedidos(pedidos);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Reporte_Ventas_{periodo}_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // GET: /Reportes/ExportarStock
    public async Task<IActionResult> ExportarStock()
    {
        var datos = await _context.ProductosTamanios
            .Include(pt => pt.Producto).ThenInclude(p => p.Categoria)
            .Include(pt => pt.Tamanio)
            .OrderBy(pt => pt.Producto.NombreProducto)
            .ThenBy(pt => pt.Tamanio.NombreTamanio)
            .ToListAsync();

        var bytes = ExcelService.ExportarStock(datos);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Reporte_Stock_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // GET: /Reportes/ExportarProductos
    public async Task<IActionResult> ExportarProductos()
    {
        var datos = await _context.Productos
            .Include(p => p.Categoria)
            .OrderBy(p => p.Categoria.NombreCategoria)
            .ThenBy(p => p.NombreProducto)
            .ToListAsync();

        var bytes = ExcelService.ExportarProductos(datos);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Reporte_Productos_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // GET: /Reportes/ExportarClientes
    public async Task<IActionResult> ExportarClientes()
    {
        var clientes = await _context.Pedidos
            .Where(p => p.Estado == "Entregado")
            .GroupBy(p => p.UsuarioId)
            .Select(g => new
            {
                UsuarioId = g.Key,
                TotalPedidos = g.Count(),
                TotalGastado = g.Sum(p => p.Total)
            })
            .OrderByDescending(x => x.TotalGastado)
            .Take(100)
            .ToListAsync();

        var users = _userManager.Users.ToList();
        var datos = clientes.Select(c =>
        {
            var u = users.FirstOrDefault(x => x.Id == c.UsuarioId);
            return (
                Nombre: $"{u?.Nombre} {u?.Apellido}".Trim(),
                Email: u?.Email ?? "",
                TotalPedidos: c.TotalPedidos,
                TotalGastado: c.TotalGastado
            );
        });

        var bytes = ExcelService.ExportarClientesFrecuentes(datos);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Reporte_Clientes_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
