using System.Text.Json;
using PryCafeteria.Models.ViewModels;

namespace PryCafeteria.Services;

/// <summary>
/// Servicio para leer y escribir el carrito en la sesión HTTP (HU11-HU13).
/// </summary>
public class CarritoService
{
    private const string SessionKey = "carrito";

    public static CarritoSesion ObtenerCarrito(ISession session)
    {
        var json = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json))
            return new CarritoSesion();
        return JsonSerializer.Deserialize<CarritoSesion>(json) ?? new CarritoSesion();
    }

    public static void GuardarCarrito(ISession session, CarritoSesion carrito)
    {
        session.SetString(SessionKey, JsonSerializer.Serialize(carrito));
    }

    public static void LimpiarCarrito(ISession session)
    {
        session.Remove(SessionKey);
    }
}
