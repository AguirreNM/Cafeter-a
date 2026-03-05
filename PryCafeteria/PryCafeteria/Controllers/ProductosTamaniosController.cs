using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;
using PryCafeteria.Services;

namespace PryCafeteria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductosTamaniosController : Controller
    {
        private readonly BdcafeteriaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductosTamaniosController(
            BdcafeteriaContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: ProductosTamanios — listado con estado de stock calculado
        // HU-Stock E2: estado dinámico En stock / Bajo stock / Agotado
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            var lista = await _context.ProductosTamanios
                .Include(p => p.Producto)
                    .ThenInclude(p => p.Categoria)
                .Include(p => p.Tamanio)
                .OrderBy(p => p.Producto.NombreProducto)
                .ThenBy(p => p.Tamanio.NombreTamanio)
                .ToListAsync();

            return View(lista);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: ProductosTamanios/Details/5
        // HU-Stock E6: historial de movimientos
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pt = await _context.ProductosTamanios
                .Include(p => p.Producto)
                .Include(p => p.Tamanio)
                .FirstOrDefaultAsync(m => m.ProductoTamanioId == id);

            if (pt == null) return NotFound();

            // Cargar historial de movimientos para este ProductoTamanio
            var movimientos = await _context.StockMovimientos
                .Where(m => m.ProductoTamanioId == id)
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            ViewBag.Movimientos = movimientos;
            return View(pt);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: ProductosTamanios/Create
        // ══════════════════════════════════════════════════════════════════════
        public IActionResult Create()
        {
            ViewData["ProductoId"] = new SelectList(
                _context.Productos.Where(p => p.Disponible), "ProductoId", "NombreProducto");
            ViewData["TamanioId"] = new SelectList(
                _context.Tamanios, "TamanioId", "NombreTamanio");
            return View();
        }

        // ══════════════════════════════════════════════════════════════════════
        // POST: ProductosTamanios/Create
        // HU-Tamaños E3 (crear asignación), HU-Stock E1 (stock inicial)
        // ══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ProductoTamanioId,ProductoId,TamanioId,Precio,Stock,Disponible")]
            ProductosTamanio productosTamanio)
        {
            ModelState.Remove("Producto");
            ModelState.Remove("Tamanio");

            if (ModelState.IsValid)
            {
                // Validar combinación única producto-tamaño
                var existe = await _context.ProductosTamanios.AnyAsync(pt =>
                    pt.ProductoId == productosTamanio.ProductoId &&
                    pt.TamanioId == productosTamanio.TamanioId);
                if (existe)
                {
                    ModelState.AddModelError("", "Ya existe esta combinación de producto y tamaño");
                    CargarListas(productosTamanio);
                    return View(productosTamanio);
                }

                // HU-Tamaños E7: precio > 0
                if (productosTamanio.Precio <= 0)
                {
                    ModelState.AddModelError("Precio", "El precio debe ser mayor a 0");
                    CargarListas(productosTamanio);
                    return View(productosTamanio);
                }

                _context.Add(productosTamanio);
                await _context.SaveChangesAsync();

                // HU-Stock E9: registrar movimiento inicial de stock
                if (productosTamanio.Stock > 0)
                {
                    var userId = _userManager.GetUserId(User);
                    _context.StockMovimientos.Add(new StockMovimiento
                    {
                        ProductoTamanioId = productosTamanio.ProductoTamanioId,
                        Tipo = "Ingreso",
                        Cantidad = productosTamanio.Stock,
                        StockResultante = productosTamanio.Stock,
                        UsuarioId = userId
                    });
                    await _context.SaveChangesAsync();
                }

                TempData["Exito"] = "Variante creada correctamente";
                return RedirectToAction(nameof(Index));
            }

            CargarListas(productosTamanio);
            return View(productosTamanio);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: ProductosTamanios/Edit/5
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pt = await _context.ProductosTamanios.FindAsync(id);
            if (pt == null) return NotFound();

            CargarListas(pt);
            return View(pt);
        }

        // ══════════════════════════════════════════════════════════════════════
        // POST: ProductosTamanios/Edit/5
        // HU-Stock E5 (reponer stock) + E9 (registrar movimiento Ajuste)
        // HU-Tamaños E4 (editar precio)
        // ══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ProductoTamanioId,ProductoId,TamanioId,Precio,Stock,Disponible")]
            ProductosTamanio productosTamanio)
        {
            ModelState.Remove("Producto");
            ModelState.Remove("Tamanio");

            if (id != productosTamanio.ProductoTamanioId) return NotFound();

            if (ModelState.IsValid)
            {
                // Validar combinación única excluyendo actual
                var existe = await _context.ProductosTamanios.AnyAsync(pt =>
                    pt.ProductoId == productosTamanio.ProductoId &&
                    pt.TamanioId == productosTamanio.TamanioId &&
                    pt.ProductoTamanioId != productosTamanio.ProductoTamanioId);
                if (existe)
                {
                    ModelState.AddModelError("", "Ya existe esta combinación de producto y tamaño");
                    CargarListas(productosTamanio);
                    return View(productosTamanio);
                }

                // HU-Tamaños E7: precio > 0
                if (productosTamanio.Precio <= 0)
                {
                    ModelState.AddModelError("Precio", "El precio debe ser mayor a 0");
                    CargarListas(productosTamanio);
                    return View(productosTamanio);
                }

                // Obtener stock anterior para calcular la diferencia
                var stockAnterior = await _context.ProductosTamanios
                    .AsNoTracking()
                    .Where(pt => pt.ProductoTamanioId == id)
                    .Select(pt => pt.Stock)
                    .FirstOrDefaultAsync();

                try
                {
                    _context.Update(productosTamanio);
                    await _context.SaveChangesAsync();

                    // HU-Stock E9: registrar movimiento de ajuste si cambió el stock
                    var diferencia = productosTamanio.Stock - stockAnterior;
                    if (diferencia != 0)
                    {
                        var userId = _userManager.GetUserId(User);
                        _context.StockMovimientos.Add(new StockMovimiento
                        {
                            ProductoTamanioId = productosTamanio.ProductoTamanioId,
                            Tipo = diferencia > 0 ? "Ingreso" : "Ajuste",
                            Cantidad = diferencia,
                            StockResultante = productosTamanio.Stock,
                            UsuarioId = userId
                        });
                        await _context.SaveChangesAsync();
                    }

                    TempData["Exito"] = "Variante actualizada correctamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductosTamanioExists(productosTamanio.ProductoTamanioId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            CargarListas(productosTamanio);
            return View(productosTamanio);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: ProductosTamanios/Delete/5
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pt = await _context.ProductosTamanios
                .Include(p => p.Producto)
                .Include(p => p.Tamanio)
                .FirstOrDefaultAsync(m => m.ProductoTamanioId == id);

            if (pt == null) return NotFound();
            return View(pt);
        }

        // ══════════════════════════════════════════════════════════════════════
        // POST: ProductosTamanios/Delete/5
        // HU-Tamaños E5/E6: bloquear si tiene ventas, sugerir "Sin stock"
        // ══════════════════════════════════════════════════════════════════════
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pt = await _context.ProductosTamanios
                .Include(p => p.DetallePedidos)
                .FirstOrDefaultAsync(p => p.ProductoTamanioId == id);

            if (pt == null) return NotFound();

            // HU-Tamaños E6: bloquear eliminación si tiene ventas
            if (pt.DetallePedidos.Any())
            {
                // Sugerir marcar como sin stock (Disponible = false)
                pt.Disponible = false;
                pt.Stock = 0;
                _context.Update(pt);
                await _context.SaveChangesAsync();
                TempData["Info"] = "Este tamaño tiene ventas registradas y fue marcado como " +
                    "\"Sin stock\" en lugar de eliminarse, preservando el historial de pedidos.";
                return RedirectToAction(nameof(Index));
            }

            // Eliminar movimientos de stock asociados primero (no hay FK restrict aquí)
            var movimientos = _context.StockMovimientos
                .Where(m => m.ProductoTamanioId == id);
            _context.StockMovimientos.RemoveRange(movimientos);

            _context.ProductosTamanios.Remove(pt);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Variante eliminada correctamente";
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET: ProductosTamanios/ExportarExcel
        // HU-Stock E7: exportar reporte de stock a Excel
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> ExportarExcel()
        {
            var datos = await _context.ProductosTamanios
                .Include(pt => pt.Producto)
                    .ThenInclude(p => p.Categoria)
                .Include(pt => pt.Tamanio)
                .OrderBy(pt => pt.Producto.NombreProducto)
                .ThenBy(pt => pt.Tamanio.NombreTamanio)
                .ToListAsync();

            var bytes = ExcelService.ExportarStock(datos);
            var fileName = $"Stock_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private bool ProductosTamanioExists(int id)
            => _context.ProductosTamanios.Any(e => e.ProductoTamanioId == id);

        private void CargarListas(ProductosTamanio? pt = null)
        {
            ViewData["ProductoId"] = new SelectList(
                _context.Productos, "ProductoId", "NombreProducto", pt?.ProductoId);
            ViewData["TamanioId"] = new SelectList(
                _context.Tamanios, "TamanioId", "NombreTamanio", pt?.TamanioId);
        }
    }
}
