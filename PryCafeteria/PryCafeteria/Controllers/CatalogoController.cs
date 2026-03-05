using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PryCafeteria.Models;
using PryCafeteria.Services;

namespace PryCafeteria.Controllers;

/// <summary>
/// Catálogo público de productos — accesible para clientes y visitantes.
/// HU06: listar productos disponibles con filtros.
/// HU07: detalle de producto con tamaños, precios y stock.
/// </summary>
public class CatalogoController : Controller
{
    private readonly BdcafeteriaContext _context;
    private readonly IMemoryCache _cache;

    public CatalogoController(BdcafeteriaContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // ══════════════════════════════════════════════════════════════════════
    // GET: /Catalogo  — HU06: catálogo con filtros
    // ══════════════════════════════════════════════════════════════════════
    public async Task<IActionResult> Index(int? categoriaId, string? busqueda)
    {
        // HU06-E6: cache de 10 min para la lista de categorías
        var categorias = await _cache.GetOrCreateAsync("cats_disponibles", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _context.Categorias
                .Where(c => c.Disponible)
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();
        });

        // Productos disponibles con eager loading
        var query = _context.Productos
            .Where(p => p.Disponible)
            .Include(p => p.Categoria)
            .Include(p => p.ProductosTamanios.Where(pt => pt.Disponible))
                .ThenInclude(pt => pt.Tamanio)
            .AsQueryable();

        // HU06-E3: filtro por categoría
        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        // HU06-E5: búsqueda por nombre o descripción
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p =>
                p.NombreProducto.Contains(busqueda) ||
                (p.Descripcion != null && p.Descripcion.Contains(busqueda)));

        var productos = await query
            .OrderBy(p => p.Categoria.NombreCategoria)
            .ThenBy(p => p.NombreProducto)
            .ToListAsync();

        // HU06-E7: mensaje cuando no hay resultados
        if (!productos.Any() && (categoriaId.HasValue || !string.IsNullOrWhiteSpace(busqueda)))
            TempData["Info"] = "No se encontraron productos con ese criterio.";

        ViewBag.Categorias = categorias;
        ViewBag.CategoriaSeleccionada = categoriaId;
        ViewBag.Busqueda = busqueda;

        // Badge carrito para el navbar
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        ViewBag.CarritoItems = carrito.TotalItems;

        return View(productos);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GET: /Catalogo/Details/5  — HU07: detalle con tamaños y stock
    // ══════════════════════════════════════════════════════════════════════
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.ProductosTamanios)
                .ThenInclude(pt => pt.Tamanio)
            .FirstOrDefaultAsync(p => p.ProductoId == id && p.Disponible);

        if (producto == null) return NotFound();

        // HU07-E6: productos relacionados de la misma categoría
        var relacionados = await _context.Productos
            .Where(p => p.CategoriaId == producto.CategoriaId
                     && p.ProductoId != producto.ProductoId
                     && p.Disponible)
            .Include(p => p.ProductosTamanios.Where(pt => pt.Disponible))
            .Take(4)
            .ToListAsync();

        ViewBag.Relacionados = relacionados;

        // Badge carrito
        var carrito = CarritoService.ObtenerCarrito(HttpContext.Session);
        ViewBag.CarritoItems = carrito.TotalItems;

        return View(producto);
    }
}
