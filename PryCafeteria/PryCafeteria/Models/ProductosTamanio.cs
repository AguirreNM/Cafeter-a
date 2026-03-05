using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models;

public partial class ProductosTamanio
{
    public int ProductoTamanioId { get; set; }

    public int ProductoId { get; set; }

    public int TamanioId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }

    public bool Disponible { get; set; } = true;

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual Producto Producto { get; set; } = null!;

    public virtual Tamanio Tamanio { get; set; } = null!;
}
