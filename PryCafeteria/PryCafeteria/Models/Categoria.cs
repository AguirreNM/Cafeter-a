using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models;

public partial class Categoria
{
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
    // HU-Categorias E7: solo letras (incluyendo acentos), espacios y guiones
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\-]+$",
        ErrorMessage = "Solo se permiten letras, espacios y guiones")]
    [Display(Name = "Categoría")]
    public string NombreCategoria { get; set; } = null!;

    // HU-Categorias E6: fecha de creación para el listado
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public bool Disponible { get; set; } = true;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
