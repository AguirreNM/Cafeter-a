using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models;

public partial class Cupone
{
    public int CuponId { get; set; }

    [Required(ErrorMessage = "El nombre del cupón es obligatorio")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre")]
    public string NombreCupon { get; set; } = null!;

        [Required(ErrorMessage = "El tipo de descuento es obligatorio")]
    [Display(Name = "Tipo")]
    public string TipoDescuento { get; set; } = null!;

    [Range(0.01, double.MaxValue, ErrorMessage = "El valor del descuento debe ser mayor a 0")]
    [Display(Name = "Valor")]
        public decimal ValorDescuento { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha final es obligatoria")]
        [Display(Name = "Fecha Final")]
        public DateTime FechaFin { get; set; }

    public bool Activo { get; set; }

    // Verifica si el cupón está vigente (activo + dentro de fechas)
    public bool EsVigente => Activo && FechaInicio <= DateTime.Now && FechaFin >= DateTime.Now;
}
