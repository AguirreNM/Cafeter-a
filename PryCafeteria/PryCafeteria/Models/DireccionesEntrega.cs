using System;
using System.Collections.Generic;

namespace PryCafeteria.Models;

public partial class DireccionesEntrega
{
    public int DireccionId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public string NombreDireccion { get; set; } = null!;

    public string Calle { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string Distrito { get; set; } = null!;

    public string CodigoPostal { get; set; } = null!;

    public string? Referencias { get; set; }

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public virtual ApplicationUser Usuario { get; set; } = null!;
}
