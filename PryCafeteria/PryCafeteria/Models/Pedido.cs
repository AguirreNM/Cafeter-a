using System;
using System.Collections.Generic;

namespace PryCafeteria.Models;

public static class EstadoPedido
{
    public const string Pendiente = "Pendiente";
    public const string EnProceso = "EnProceso";
    public const string Listo = "Listo";
    public const string Entregado = "Entregado";
    public const string Cancelado = "Cancelado";

    public static readonly string[] Todos = [Pendiente, EnProceso, Listo, Entregado, Cancelado];
}

public static class TipoEntrega
{
    public const string Recojo = "Recojo";
    public const string Delivery = "Delivery";

    public static readonly string[] Todos = [Recojo, Delivery];
}

public partial class Pedido
{
    public int PedidoId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public DateTime FechaPedido { get; set; }

    public decimal Descuento { get; set; }

    public decimal Total { get; set; }

    public int MetodoPagoId { get; set; }

    public int? DireccionId { get; set; }

    public string TipoEntrega { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual DireccionesEntrega? Direccion { get; set; }

    public virtual MetodosPago MetodoPago { get; set; } = null!;

    public virtual ApplicationUser Usuario { get; set; } = null!;
}
