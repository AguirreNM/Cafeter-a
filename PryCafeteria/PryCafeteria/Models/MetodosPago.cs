using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models;

public partial class MetodosPago
{
    public int MetodoPagoId { get; set; }

    [Required(ErrorMessage = "El nombre del método de pago es obligatorio")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
    [Display(Name = "Nombre")]
    public string NombreMetodoPago { get; set; } = null!;

    public bool Disponible { get; set; } = true;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
