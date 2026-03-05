using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models;

public partial class Tamanio
{
    public int TamanioId { get; set; }

    [Required(ErrorMessage = "El nombre del tamaño es obligatorio")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 30 caracteres")]
    [Display(Name = "Tamaño")]
    public string NombreTamanio { get; set; } = null!;

    public virtual ICollection<ProductosTamanio> ProductosTamanios { get; set; } = new List<ProductosTamanio>();
}
