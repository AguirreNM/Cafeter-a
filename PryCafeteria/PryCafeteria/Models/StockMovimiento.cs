using System;
using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models;

/// <summary>
/// HU-Stock E9: auditoría de cada variación de stock (Ingreso, Venta, Ajuste)
/// </summary>
public class StockMovimiento
{
    public int StockMovimientoId { get; set; }

    public int ProductoTamanioId { get; set; }

    [Required]
    [StringLength(20)]
    public string Tipo { get; set; } = null!; // "Ingreso" | "Venta" | "Ajuste"

    public int Cantidad { get; set; }          // positivo = entrada, negativo = salida

    public int StockResultante { get; set; }

    [StringLength(200)]
    public string? Referencia { get; set; }   // "Pedido #123", "Cancelación pedido #45", etc.

    public DateTime Fecha { get; set; } = DateTime.Now;

    // Quién hizo el movimiento (userId de Identity o "Sistema" para triggers EF)
    [StringLength(450)]
    public string? UsuarioId { get; set; }

    // Navegación
    public virtual ProductosTamanio ProductoTamanio { get; set; } = null!;
    public virtual ApplicationUser? Usuario { get; set; }
}
