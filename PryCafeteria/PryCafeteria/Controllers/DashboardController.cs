using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PryCafeteria.Models;

namespace PryCafeteria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly BdcafeteriaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        // Duración del caché: HU-Dashboard E15
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

        public DashboardController(
            BdcafeteriaContext context,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Dashboard/Index
        // HU-Dashboard: E1 tarjetas, E3 actividad reciente, E6 alerta stock,
        //               E7 pedidos pendientes, E15 caché 2 min
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var ayer = hoy.AddDays(-1);

            // ── Caché de métricas básicas (2 minutos) ──────────────────────
            if (!_cache.TryGetValue("dash_metricas", out DashMetricas? metricas))
            {
                metricas = new DashMetricas
                {
                    // Pedidos del día
                    PedidosHoy = await _context.Pedidos
                        .CountAsync(p => p.FechaPedido >= hoy),

                    PedidosAyer = await _context.Pedidos
                        .CountAsync(p => p.FechaPedido >= ayer && p.FechaPedido < hoy),

                    // Ingresos del día (solo pedidos Entregados)
                    IngresosHoy = await _context.Pedidos
                        .Where(p => p.FechaPedido >= hoy && p.Estado == "Entregado")
                        .SumAsync(p => (decimal?)p.Total) ?? 0,

                    IngresosAyer = await _context.Pedidos
                        .Where(p => p.FechaPedido >= ayer && p.FechaPedido < hoy && p.Estado == "Entregado")
                        .SumAsync(p => (decimal?)p.Total) ?? 0,

                    // Pedidos pendientes
                    PedidosPendientes = await _context.Pedidos
                        .CountAsync(p => p.Estado == "Pendiente"),

                    // Stock bajo y agotado
                    StockBajoCount = await _context.ProductosTamanios
                        .CountAsync(pt => pt.Stock > 0 && pt.Stock <= 5),

                    AgotadosCount = await _context.ProductosTamanios
                        .CountAsync(pt => pt.Stock == 0),

                    // Totales catálogo
                    TotalProductos = await _context.Productos.CountAsync(),
                    TotalCategorias = await _context.Categorias.CountAsync(),
                    TotalCupones = await _context.Cupones.CountAsync(c => c.Activo == true),
                };

                _cache.Set("dash_metricas", metricas, CacheDuration);
            }

            // ── Variación porcentual pedidos día vs ayer ────────────────────
            double varPedidos = 0;
            if (metricas!.PedidosAyer > 0)
                varPedidos = Math.Round(
                    (metricas.PedidosHoy - metricas.PedidosAyer) * 100.0 / metricas.PedidosAyer, 1);

            double varIngresos = 0;
            if (metricas.IngresosAyer > 0)
                varIngresos = Math.Round(
                    (double)((metricas.IngresosHoy - metricas.IngresosAyer) * 100 / metricas.IngresosAyer), 1);

            // ── Últimos 10 pedidos (actividad reciente) — no cacheados ──────
            var ultimosPedidos = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.MetodoPago)
                .OrderByDescending(p => p.FechaPedido)
                .Take(10)
                .ToListAsync();

            // ── Últimos 5 usuarios registrados ──────────────────────────────
            var ultimosUsuarios = _userManager.Users
                .OrderByDescending(u => u.FechaRegistro)
                .Take(5)
                .ToList();

            // ── Productos con stock bajo para banner de alerta ───────────────
            var productosStockBajo = await _context.ProductosTamanios
                .Include(pt => pt.Producto)
                .Include(pt => pt.Tamanio)
                .Where(pt => pt.Stock <= 5)
                .OrderBy(pt => pt.Stock)
                .Take(20)
                .ToListAsync();

            // ── Nuevos usuarios del día ──────────────────────────────────────
            var nuevosHoy = _userManager.Users
                .Count(u => u.FechaRegistro >= hoy);

            // ── Pasar todo al ViewBag ────────────────────────────────────────
            ViewBag.PedidosHoy        = metricas.PedidosHoy;
            ViewBag.PedidosAyer       = metricas.PedidosAyer;
            ViewBag.VarPedidos        = varPedidos;
            ViewBag.IngresosHoy       = metricas.IngresosHoy;
            ViewBag.VarIngresos       = varIngresos;
            ViewBag.PedidosPendientes = metricas.PedidosPendientes;
            ViewBag.StockBajo         = metricas.StockBajoCount;
            ViewBag.Agotados          = metricas.AgotadosCount;
            ViewBag.TotalProductos    = metricas.TotalProductos;
            ViewBag.TotalCategorias   = metricas.TotalCategorias;
            ViewBag.TotalCupones      = metricas.TotalCupones;
            ViewBag.NuevosHoy         = nuevosHoy;

            // Últimos 5 productos para la sección del dashboard
            var ultimosProductos = await _context.Productos
                .Include(p => p.Categoria)
                .OrderByDescending(p => p.ProductoId)
                .Take(5)
                .ToListAsync();

            ViewBag.UltimosPedidos      = ultimosPedidos;
            ViewBag.UltimosUsuarios     = ultimosUsuarios;
            ViewBag.UltimosProductos    = ultimosProductos;
            ViewBag.ProductosStockBajo  = productosStockBajo;

            return View();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ══════════════════════════════════════════════════════════════════════
        // GET: Dashboard/Cocina — HU-Dashboard E9: vista tiempo real de pedidos activos
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Cocina()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.DetallePedidos)
                .Where(p => p.Estado == EstadoPedido.Pendiente
                         || p.Estado == EstadoPedido.EnProceso)
                .OrderBy(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Dashboard/CocinaData — endpoint JSON para polling (HU-Dashboard E9)
        // ══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> CocinaData()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.DetallePedidos)
                .Where(p => p.Estado == EstadoPedido.Pendiente
                         || p.Estado == EstadoPedido.EnProceso)
                .OrderBy(p => p.FechaPedido)
                .Select(p => new
                {
                    p.PedidoId,
                    p.Estado,
                    p.TipoEntrega,
                    FechaPedido = p.FechaPedido.ToString("HH:mm"),
                    Cliente = p.Usuario != null ? $"{p.Usuario.Nombre} {p.Usuario.Apellido}".Trim() : "—",
                    Items = p.DetallePedidos.Select(d => new
                    {
                        d.NombreProductoSnapshot,
                        d.NombreTamanioSnapshot,
                        d.Cantidad
                    }).ToList()
                })
                .ToListAsync();

            return Json(pedidos);
        }

        // POST: Dashboard/AvanzarEstadoCocina — cambio de estado desde cocina
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvanzarEstadoCocina(int pedidoId)
        {
            var pedido = await _context.Pedidos.FindAsync(pedidoId);
            if (pedido == null) return NotFound();

            pedido.Estado = pedido.Estado switch
            {
                EstadoPedido.Pendiente  => EstadoPedido.EnProceso,
                EstadoPedido.EnProceso  => EstadoPedido.Listo,
                _                      => pedido.Estado
            };

            await _context.SaveChangesAsync();
            _cache.Remove("dash_metricas"); // invalidar caché
            return Json(new { ok = true, nuevoEstado = pedido.Estado });
        }

        // POST: Dashboard/InvalidarCache — fuerza actualización de métricas
        // ══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InvalidarCache()
        {
            _cache.Remove("dash_metricas");
            TempData["Exito"] = "Métricas del dashboard actualizadas";
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Dashboard/DatosVentas7Dias — endpoint para Chart.js (HU-Dashboard E2)
        // ══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> DatosVentas7Dias()
        {
            var desde = DateTime.Today.AddDays(-6);

            var ventas = await _context.Pedidos
                .Where(p => p.FechaPedido >= desde && p.Estado == "Entregado")
                .GroupBy(p => p.FechaPedido.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Total = g.Sum(p => p.Total),
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            // Completar días sin ventas con cero
            var resultado = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .Select(d => new
                {
                    label = d.ToString("ddd dd/MM"),
                    total = ventas.FirstOrDefault(v => v.Fecha == d)?.Total ?? 0,
                    cantidad = ventas.FirstOrDefault(v => v.Fecha == d)?.Cantidad ?? 0
                })
                .ToList();

            return Json(resultado);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Dashboard/DatosTopProductos — para gráfico barras (HU-Dashboard E2)
        // ══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> DatosTopProductos()
        {
            var top = await _context.DetallePedidos
                .GroupBy(d => d.NombreProductoSnapshot)
                .Select(g => new
                {
                    nombre = g.Key ?? "—",
                    cantidad = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.cantidad)
                .Take(8)
                .ToListAsync();

            return Json(top);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Dashboard/DatosPorCategoria — para gráfico pie (HU-Dashboard E2)
        // ══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> DatosPorCategoria()
        {
            var datos = await _context.DetallePedidos
                .Include(d => d.ProductoTamanio)
                    .ThenInclude(pt => pt.Producto)
                        .ThenInclude(p => p.Categoria)
                .GroupBy(d => d.ProductoTamanio.Producto.Categoria.NombreCategoria)
                .Select(g => new
                {
                    categoria = g.Key,
                    total = g.Sum(d => d.Cantidad * (double)d.PrecioUnitario)
                })
                .OrderByDescending(x => x.total)
                .ToListAsync();

            return Json(datos);
        }
    }

    // ── DTO interno para caché ───────────────────────────────────────────────
    internal class DashMetricas
    {
        public int PedidosHoy       { get; set; }
        public int PedidosAyer      { get; set; }
        public decimal IngresosHoy  { get; set; }
        public decimal IngresosAyer { get; set; }
        public int PedidosPendientes { get; set; }
        public int StockBajoCount   { get; set; }
        public int AgotadosCount    { get; set; }
        public int TotalProductos   { get; set; }
        public int TotalCategorias  { get; set; }
        public int TotalCupones     { get; set; }
    }
}
