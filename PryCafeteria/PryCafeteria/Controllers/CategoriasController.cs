using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;

namespace PryCafeteria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriasController : Controller
    {
        private readonly BdcafeteriaContext _context;

        public CategoriasController(BdcafeteriaContext context)
        {
            _context = context;
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.Include(c => c.Productos).ToListAsync());
        }

        // GET: Categorias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Include Productos para que la vista pueda mostrar el conteo sin lazy-load exception
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(m => m.CategoriaId == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoriaId,NombreCategoria,Disponible")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                // HU05 - E2: validar nombre duplicado
                var existe = await _context.Categorias.AnyAsync(c =>
                    c.NombreCategoria.ToLower() == categoria.NombreCategoria.ToLower());
                if (existe)
                {
                    ModelState.AddModelError("NombreCategoria", "Esta categoría ya existe");
                    return View(categoria);
                }

                _context.Add(categoria);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Categoría creada correctamente";
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }

        // POST: Categorias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,NombreCategoria,Disponible")] Categoria categoria)
        {
            if (id != categoria.CategoriaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // HU05 - E2: validar nombre duplicado excluyendo la actual
                var existe = await _context.Categorias.AnyAsync(c =>
                    c.NombreCategoria.ToLower() == categoria.NombreCategoria.ToLower() &&
                    c.CategoriaId != categoria.CategoriaId);
                if (existe)
                {
                    ModelState.AddModelError("NombreCategoria", "Esta categoría ya existe");
                    return View(categoria);
                }

                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.CategoriaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Exito"] = "Categoría actualizada correctamente";
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.CategoriaId == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.CategoriaId == id);

            if (categoria == null)
                return NotFound();

            // HU05 - E5: bloquear si tiene productos
            if (categoria.Productos.Any())
            {
                TempData["Error"] = $"No se puede eliminar: tiene {categoria.Productos.Count} producto(s) asociado(s)";
                return RedirectToAction(nameof(Index));
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Categoría eliminada correctamente";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.CategoriaId == id);
        }
    }
}
