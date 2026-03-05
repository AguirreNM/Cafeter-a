using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models;

public partial class Producto
{
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    [Display(Name = "Producto")]
    public string NombreProducto { get; set; } = null!;

    [StringLength(200, ErrorMessage = "La descripción no puede superar 200 caracteres")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public int CategoriaId { get; set; }

    public string? Imagen { get; set; }

    public bool Disponible { get; set; }

    [Display(Name = "Categoría")]
    public virtual Categoria Categoria { get; set; } = null!;

    public virtual ICollection<ProductosTamanio> ProductosTamanios { get; set; } = new List<ProductosTamanio>();
}
