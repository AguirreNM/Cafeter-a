using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;
using PryCafeteria.Services;

namespace PryCafeteria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductosController : Controller
    {
        private readonly BdcafeteriaContext _context;
        private readonly IImagenService _imagenService;

        // Ruta del placeholder cuando no se sube imagen — HU-Productos E2
        private const string PlaceholderPath = "/images/placeholder-producto.png";

        public ProductosController(BdcafeteriaContext context, IImagenService imagenService)
        {
            _context = context;
            _imagenService = imagenService;
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Productos
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .ToListAsync();
            return View(productos);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Productos/Details/5
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.ProductosTamanios)
                    .ThenInclude(pt => pt.Tamanio)
                .FirstOrDefaultAsync(m => m.ProductoId == id);

            if (producto == null) return NotFound();
            return View(producto);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Productos/Create
        // ══════════════════════════════════════════════════════════════════════
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias.Where(c => c.Disponible),
                "CategoriaId", "NombreCategoria");
            return View();
        }

        // ══════════════════════════════════════════════════════════════════════
        // POST: Productos/Create
        // HU-Productos: E1 (crear), E2 (placeholder), E8 (nombre único por cat),
        //               E9 (validar imagen), E10 (resize 800x800)
        // ══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("NombreProducto,Descripcion,CategoriaId,Disponible")] Producto producto,
            IFormFile? imagenArchivo)
        {
            ModelState.Remove("Categoria");
            ModelState.Remove("Imagen");

            if (ModelState.IsValid)
            {
                // ── E8: nombre único por categoría ──────────────────────────
                var existe = await _context.Productos.AnyAsync(p =>
                    p.NombreProducto.ToLower() == producto.NombreProducto.ToLower() &&
                    p.CategoriaId == producto.CategoriaId);
                if (existe)
                {
                    ModelState.AddModelError("NombreProducto",
                        "Ya existe un producto con este nombre en la misma categoría");
                    CargarCategorias(producto.CategoriaId);
                    return View(producto);
                }

                // ── E1/E2: procesar imagen o asignar placeholder ─────────────
                if (imagenArchivo != null && imagenArchivo.Length > 0)
                {
                    var categoria = await _context.Categorias.FindAsync(producto.CategoriaId);
                    var (ruta, error) = await _imagenService
                        .GuardarAsync(imagenArchivo, categoria?.NombreCategoria ?? "General");

                    if (error != null)
                    {
                        ModelState.AddModelError("imagenArchivo", error);
                        CargarCategorias(producto.CategoriaId);
                        return View(producto);
                    }
                    producto.Imagen = ruta;
                }
                else
                {
                    // E2: placeholder por defecto
                    producto.Imagen = PlaceholderPath;
                }

                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Producto creado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Productos/Edit/5
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        // ══════════════════════════════════════════════════════════════════════
        // POST: Productos/Edit/5
        // HU-Productos: E3 (conservar imagen), E4 (reemplazar imagen),
        //               E8 (nombre único por cat)
        // ══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ProductoId,NombreProducto,Descripcion,CategoriaId,Disponible")] Producto producto,
            IFormFile? imagenArchivo,
            string? imagenActual)
        {
            ModelState.Remove("Categoria");
            ModelState.Remove("Imagen");

            if (id != producto.ProductoId) return NotFound();

            if (ModelState.IsValid)
            {
                // ── E8: nombre único por categoría excluyendo el actual ──────
                var existe = await _context.Productos.AnyAsync(p =>
                    p.NombreProducto.ToLower() == producto.NombreProducto.ToLower() &&
                    p.CategoriaId == producto.CategoriaId &&
                    p.ProductoId != producto.ProductoId);
                if (existe)
                {
                    ModelState.AddModelError("NombreProducto",
                        "Ya existe un producto con este nombre en la misma categoría");
                    CargarCategorias(producto.CategoriaId);
                    return View(producto);
                }

                // ── E3/E4: gestión de imagen ─────────────────────────────────
                if (imagenArchivo != null && imagenArchivo.Length > 0)
                {
                    // E9/E10: validar y redimensionar nueva imagen
                    var categoria = await _context.Categorias.FindAsync(producto.CategoriaId);
                    var (ruta, error) = await _imagenService
                        .GuardarAsync(imagenArchivo, categoria?.NombreCategoria ?? "General");

                    if (error != null)
                    {
                        ModelState.AddModelError("imagenArchivo", error);
                        CargarCategorias(producto.CategoriaId);
                        return View(producto);
                    }

                    // E4: eliminar imagen anterior del servidor
                    if (!string.IsNullOrEmpty(imagenActual) && imagenActual != PlaceholderPath)
                        _imagenService.Eliminar(imagenActual);

                    producto.Imagen = ruta;
                }
                else
                {
                    // E3: conservar imagen original si no se subió una nueva
                    producto.Imagen = imagenActual ?? PlaceholderPath;
                }

                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = "Producto actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.ProductoId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: Productos/Delete/5
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.ProductoId == id);

            if (producto == null) return NotFound();
            return View(producto);
        }

        // ══════════════════════════════════════════════════════════════════════
        // POST: Productos/Delete/5
        // HU-Productos: E5 (eliminar sin ventas), E6/E11 (soft delete con ventas)
        // ══════════════════════════════════════════════════════════════════════
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.ProductosTamanios)
                    .ThenInclude(pt => pt.DetallePedidos)
                .FirstOrDefaultAsync(p => p.ProductoId == id);

            if (producto == null) return NotFound();

            // E6/E11: si tiene ventas → soft delete
            var tieneVentas = producto.ProductosTamanios.Any(pt => pt.DetallePedidos.Any());
            if (tieneVentas)
            {
                producto.Disponible = false;
                _context.Update(producto);
                await _context.SaveChangesAsync();
                TempData["Info"] = "El producto tiene ventas registradas y fue marcado como " +
                    "\"No disponible\". No se puede eliminar permanentemente para preservar el historial.";
                return RedirectToAction(nameof(Index));
            }

            // E5: eliminación física cuando no hay ventas
            if (!string.IsNullOrEmpty(producto.Imagen) && producto.Imagen != PlaceholderPath)
                _imagenService.Eliminar(producto.Imagen);

            _context.ProductosTamanios.RemoveRange(producto.ProductosTamanios);
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Producto eliminado correctamente";
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // POST: Productos/ToggleDisponible/5
        // HU-Productos E7: toggle rápido de disponibilidad desde el listado
        // ══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDisponible(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            producto.Disponible = !producto.Disponible;
            await _context.SaveChangesAsync();

            TempData["Exito"] = producto.Disponible
                ? $"'{producto.NombreProducto}' marcado como Disponible"
                : $"'{producto.NombreProducto}' marcado como No disponible";

            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private bool ProductoExists(int id)
            => _context.Productos.Any(e => e.ProductoId == id);

        private void CargarCategorias(int? selectedId = null)
        {
            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias.Where(c => c.Disponible),
                "CategoriaId", "NombreCategoria", selectedId);
        }
    }
}
