using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;

namespace PryCafeteria.Filters
{
    /// <summary>
    /// Filtro global que inyecta en el sidebar dos badges para Admin:
    ///   • ViewBag.StockBajoSidebar    — variantes con stock &lt;= 5
    ///   • ViewBag.PedidosPendSidebar  — pedidos en estado "Pendiente"
    /// Solo aplica cuando el usuario es Admin.
    /// </summary>
    public class StockBajoFilter : IAsyncActionFilter
    {
        private readonly BdcafeteriaContext _context;

        public StockBajoFilter(BdcafeteriaContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is Controller controller
                && context.HttpContext.User.Identity?.IsAuthenticated == true
                && context.HttpContext.User.IsInRole("Admin"))
            {
                try
                {
                    // Badge stock bajo
                    int stockBajo = await _context.ProductosTamanios
                        .CountAsync(pt => pt.Stock <= 5);
                    controller.ViewBag.StockBajoSidebar = stockBajo;

                    // Badge pedidos pendientes
                    int pedidosPend = await _context.Pedidos
                        .CountAsync(p => p.Estado == EstadoPedido.Pendiente);
                    controller.ViewBag.PedidosPendSidebar = pedidosPend;
                }
                catch
                {
                    controller.ViewBag.StockBajoSidebar = 0;
                    controller.ViewBag.PedidosPendSidebar = 0;
                }
            }

            await next();
        }
    }
}
