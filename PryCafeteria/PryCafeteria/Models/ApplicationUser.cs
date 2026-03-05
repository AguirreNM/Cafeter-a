using Microsoft.AspNetCore.Identity;

namespace PryCafeteria.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public virtual ICollection<DireccionesEntrega> DireccionesEntregas { get; set; } = new List<DireccionesEntrega>();
    }
}
