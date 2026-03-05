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
    public class CuponesController : Controller
    {
        private readonly BdcafeteriaContext _context;

        public CuponesController(BdcafeteriaContext context)
        {
            _context = context;
        }

        // GET: Cupones
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cupones.ToListAsync());
        }

        // GET: Cupones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupone = await _context.Cupones
                .FirstOrDefaultAsync(m => m.CuponId == id);
            if (cupone == null)
            {
                return NotFound();
            }

            return View(cupone);
        }

        // GET: Cupones/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cupones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CuponId,NombreCupon,TipoDescuento,ValorDescuento,FechaInicio,FechaFin,Activo")] Cupone cupone)
        {
            if (ModelState.IsValid)
            {
                // Validar nombre duplicado
                var existe = await _context.Cupones.AnyAsync(c =>
                    c.NombreCupon.ToLower() == cupone.NombreCupon.ToLower());
                if (existe)
                {
                    ModelState.AddModelError("NombreCupon", "Ya existe un cupón con este nombre");
                    return View(cupone);
                }

                _context.Add(cupone);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Cupón creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            return View(cupone);
        }

        // GET: Cupones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupone = await _context.Cupones.FindAsync(id);
            if (cupone == null)
            {
                return NotFound();
            }
            return View(cupone);
        }

        // POST: Cupones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CuponId,NombreCupon,TipoDescuento,ValorDescuento,FechaInicio,FechaFin,Activo")] Cupone cupone)
        {
            if (id != cupone.CuponId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validar nombre duplicado excluyendo el actual
                var existe = await _context.Cupones.AnyAsync(c =>
                    c.NombreCupon.ToLower() == cupone.NombreCupon.ToLower() &&
                    c.CuponId != cupone.CuponId);
                if (existe)
                {
                    ModelState.AddModelError("NombreCupon", "Ya existe un cupón con este nombre");
                    return View(cupone);
                }

                try
                {
                    _context.Update(cupone);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = "Cupón actualizado correctamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuponeExists(cupone.CuponId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cupone);
        }

        // GET: Cupones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupone = await _context.Cupones
                .FirstOrDefaultAsync(m => m.CuponId == id);
            if (cupone == null)
            {
                return NotFound();
            }

            return View(cupone);
        }

        // POST: Cupones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cupone = await _context.Cupones.FindAsync(id);
            if (cupone == null)
                return NotFound();

            _context.Cupones.Remove(cupone);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Cupón eliminado correctamente";
            return RedirectToAction(nameof(Index));
        }

        private bool CuponeExists(int id)
        {
            return _context.Cupones.Any(e => e.CuponId == id);
        }
    }
}
