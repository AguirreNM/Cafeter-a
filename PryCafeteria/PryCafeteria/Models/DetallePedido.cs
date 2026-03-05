using System;
using System.Collections.Generic;

namespace PryCafeteria.Models;

public partial class DetallePedido
{
    public int DetalleId { get; set; }

    public int PedidoId { get; set; }

    public int ProductoTamanioId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    // Snapshot del nombre al momento del pedido (preserva historial si el producto cambia)
    public string? NombreProductoSnapshot { get; set; }
    public string? NombreTamanioSnapshot { get; set; }

    public virtual Pedido Pedido { get; set; } = null!;

    public virtual ProductosTamanio ProductoTamanio { get; set; } = null!;
}
