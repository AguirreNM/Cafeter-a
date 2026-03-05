using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PryCafeteria.Controllers;

public class ErrorController : Controller
{
    // GET: /Error/404 — Página no encontrada
    [Route("Error/404")]
    public IActionResult NotFound404()
    {
        Response.StatusCode = 404;
        return View("Error404");
    }

    // GET: /Error/500 — Error interno del servidor
    [Route("Error/500")]
    public IActionResult ServerError()
    {
        Response.StatusCode = 500;
        var ex = HttpContext.Features.Get<IExceptionHandlerFeature>();
        // ex?.Error  ← disponible para logging si se necesita
        return View("Error500");
    }

    // Catch-all para otros códigos (403, 401, etc.)
    [Route("Error/{code:int}")]
    public IActionResult GenericError(int code)
    {
        Response.StatusCode = code;
        return code switch
        {
            404 => View("Error404"),
            500 => View("Error500"),
            _   => View("Error404")  // fallback
        };
    }
}
