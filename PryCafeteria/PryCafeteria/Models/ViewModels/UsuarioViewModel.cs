using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Entre 2 y 50 caracteres")]
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Entre 2 y 50 caracteres")]
        [Display(Name = "Apellido")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        [Display(Name = "Rol")]
        public string? Rol { get; set; }

        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

        public DateTime? FechaRegistro { get; set; }
        public int TotalPedidos { get; set; }

        /// <summary>HU03 - E9: true = cuenta bloqueada indefinidamente (soft delete)</summary>
        public bool EstaBloqueado { get; set; }
    }
}
