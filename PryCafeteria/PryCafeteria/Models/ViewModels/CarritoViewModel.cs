using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models.ViewModels;

/// <summary>
/// Representa un ítem dentro del carrito de sesión.
/// </summary>
public class CarritoItem
{
    public int ProductoTamanioId { get; set; }
    public string NombreProducto { get; set; } = "";
    public string NombreTamanio { get; set; } = "";
    public string? Imagen { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public int StockDisponible { get; set; }

    public decimal Subtotal => PrecioUnitario * Cantidad;
}

/// <summary>
/// Estado completo del carrito (se serializa en Session).
/// </summary>
public class CarritoSesion
{
    public List<CarritoItem> Items { get; set; } = new();
    public string? CuponCodigo { get; set; }
    public decimal Descuento { get; set; }

    public decimal Subtotal => Items.Sum(i => i.Subtotal);
    public decimal Total => Math.Max(0, Subtotal - Descuento);
    public int TotalItems => Items.Sum(i => i.Cantidad);
}

/// <summary>
/// ViewModel para la vista del carrito.
/// </summary>
public class CarritoIndexViewModel
{
    public CarritoSesion Carrito { get; set; } = new();
    public string? MensajeCupon { get; set; }
    public bool CuponExitoso { get; set; }
}
