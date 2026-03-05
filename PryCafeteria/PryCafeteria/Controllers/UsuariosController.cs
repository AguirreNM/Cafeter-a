using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;
using PryCafeteria.Models.ViewModels;

namespace PryCafeteria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly BdcafeteriaContext _context;

        public UsuariosController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, BdcafeteriaContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Usuarios
        // HU03-E12: paginación de 20 registros
        public async Task<IActionResult> Index(string? buscar, string? tab, int pagina = 1)
        {
            const int PageSize = 20;
            tab = tab ?? "admins";

            // ── Obtener roles con una sola consulta JOIN (evita N+1) ──────────
            // Consulta directa a las tablas de Identity para obtener usuario+rol en una query
            var rolObjetivo = tab == "admins" ? "Admin" : "Cliente";

            var usuariosConRol = await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                where r.Name == rolObjetivo
                select new
                {
                    u.Id, u.Nombre, u.Apellido, u.Email,
                    u.FechaRegistro, u.LockoutEnd, u.LockoutEnabled,
                    Rol = r.Name
                }
            ).ToListAsync();

            // ── Filtrado por búsqueda (server-side para paginación correcta) ──
            if (!string.IsNullOrEmpty(buscar))
            {
                var b = buscar.ToLower();
                usuariosConRol = usuariosConRol.Where(u =>
                    (u.Nombre != null && u.Nombre.ToLower().Contains(b)) ||
                    (u.Apellido != null && u.Apellido.ToLower().Contains(b)) ||
                    (u.Email != null && u.Email.ToLower().Contains(b))
                ).ToList();
            }

            var totalRegistros = usuariosConRol.Count;
            var totalPaginas   = (int)Math.Ceiling(totalRegistros / (double)PageSize);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, totalPaginas)));

            // ── Obtener todos los pedidosCount de estos usuarios en UNA query ─
            var ids = usuariosConRol.Select(u => u.Id).ToList();
            var pedidosPorUsuario = await _context.Pedidos
                .Where(p => ids.Contains(p.UsuarioId))
                .GroupBy(p => p.UsuarioId)
                .Select(g => new { UsuarioId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UsuarioId, x => x.Count);

            var paginated = usuariosConRol
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize);

            var lista = paginated.Select(u => new UsuarioViewModel
            {
                Id           = u.Id,
                Nombre       = u.Nombre,
                Apellido     = u.Apellido,
                Email        = u.Email,
                Rol          = u.Rol ?? "Sin rol",
                FechaRegistro = u.FechaRegistro,
                TotalPedidos = pedidosPorUsuario.GetValueOrDefault(u.Id, 0),
                EstaBloqueado = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
            }).ToList();

            ViewBag.Buscar       = buscar;
            ViewBag.Tab          = tab;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;
            return View(lista);
        }

        // GET: Usuarios/Crear
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string> { "Admin", "Cliente" };
            return View();
        }

        // POST: Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioViewModel modelo)
        {
            if (string.IsNullOrEmpty(modelo.Password))
                ModelState.AddModelError("Password", "La contraseña es obligatoria al crear un usuario");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "Admin", "Cliente" };
                return View(modelo);
            }

            // Verificar email duplicado
            var existente = await _userManager.FindByEmailAsync(modelo.Email!);
            if (existente != null)
            {
                ModelState.AddModelError("Email", "Este correo ya está registrado");
                ViewBag.Roles = new List<string> { "Admin", "Cliente" };
                return View(modelo);
            }

            var usuario = new ApplicationUser
            {
                UserName = modelo.Email,
                Email = modelo.Email,
                Nombre = modelo.Nombre,
                Apellido = modelo.Apellido,
                EmailConfirmed = true,
                FechaRegistro = DateTime.Now
            };

            var resultado = await _userManager.CreateAsync(usuario, modelo.Password!);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(usuario, modelo.Rol!);
                TempData["Exito"] = "Usuario creado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in resultado.Errors)
                ModelState.AddModelError("", error.Description);

            ViewBag.Roles = new List<string> { "Admin", "Cliente" };
            return View(modelo);
        }

        // GET: Usuarios/Editar/id
        public async Task<IActionResult> Edit(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(usuario);

            var modelo = new UsuarioViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rol = roles.FirstOrDefault() ?? "Sin rol"
            };

            ViewBag.Roles = new List<string> { "Admin", "Cliente" };
            return View(modelo);
        }

        // POST: Usuarios/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioViewModel modelo)
        {
            // Password no es requerido al editar
            ModelState.Remove("Password");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "Admin", "Cliente" };
                return View(modelo);
            }

            var usuario = await _userManager.FindByIdAsync(modelo.Id!);
            if (usuario == null) return NotFound();

            usuario.Nombre = modelo.Nombre;
            usuario.Apellido = modelo.Apellido;
            usuario.Email = modelo.Email;
            usuario.UserName = modelo.Email;

            await _userManager.UpdateAsync(usuario);

            // Actualizar rol
            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, modelo.Rol!);

            TempData["Exito"] = "Usuario actualizado correctamente";
            return RedirectToAction(nameof(Index));
        }

        // POST: Usuarios/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            // HU03 - E6 / E9: si tiene pedidos, aplicar soft delete (bloquear cuenta)
            var tienePedidos = await _context.Pedidos.AnyAsync(p => p.UsuarioId == id);
            if (tienePedidos)
            {
                // Soft delete: bloquear permanentemente la cuenta conservando el registro
                usuario.LockoutEnabled = true;
                usuario.LockoutEnd = DateTimeOffset.MaxValue;
                await _userManager.UpdateAsync(usuario);
                TempData["Info"] = "No se puede eliminar: el usuario tiene pedidos asociados. La cuenta fue desactivada y ya no podrá iniciar sesión.";
                return RedirectToAction(nameof(Index));
            }

            // Eliminación física solo cuando no tiene pedidos
            await _userManager.DeleteAsync(usuario);
            TempData["Exito"] = "Usuario eliminado correctamente";
            return RedirectToAction(nameof(Index));
        }
    }
}