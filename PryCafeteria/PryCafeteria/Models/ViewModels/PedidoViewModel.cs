using System.ComponentModel.DataAnnotations;

namespace PryCafeteria.Models.ViewModels;

/// <summary>
/// ViewModel para el paso de Checkout (HU14).
/// </summary>
public class CheckoutViewModel
{
    [Required(ErrorMessage = "Seleccione un método de pago")]
    [Display(Name = "Método de pago")]
    public int MetodoPagoId { get; set; }

    [Required(ErrorMessage = "Seleccione el tipo de entrega")]
    [Display(Name = "Tipo de entrega")]
    public string TipoEntrega { get; set; } = PryCafeteria.Models.TipoEntrega.Recojo;

    // Solo obligatorio si TipoEntrega == Delivery
    [Display(Name = "Dirección de entrega")]
    public int? DireccionId { get; set; }

    // Para crear nueva dirección en el mismo checkout
    public NuevaDireccionViewModel? NuevaDireccion { get; set; }

    // Datos de solo lectura para mostrar el resumen
    public CarritoSesion? Carrito { get; set; }
    public List<MetodosPago> MetodosPago { get; set; } = new();
    public List<DireccionesEntrega> Direcciones { get; set; } = new();
}

/// <summary>
/// Sub-VM para capturar nueva dirección inline en checkout.
/// </summary>
public class NuevaDireccionViewModel
{
    [StringLength(50)]
    [Display(Name = "Nombre de referencia")]
    public string? NombreDireccion { get; set; }

    [StringLength(100)]
    [Display(Name = "Calle")]
    public string? Calle { get; set; }

    [StringLength(20)]
    [Display(Name = "Número")]
    public string? Numero { get; set; }

    [StringLength(100)]
    [Display(Name = "Distrito")]
    public string? Distrito { get; set; }

    [StringLength(10)]
    [Display(Name = "Código postal")]
    public string? CodigoPostal { get; set; }

    [StringLength(200)]
    [Display(Name = "Referencias")]
    public string? Referencias { get; set; }
}

/// <summary>
/// ViewModel para la página "Mis pedidos" del cliente (HU15).
/// </summary>
public class MisPedidosViewModel
{
    public List<Pedido> Pedidos { get; set; } = new();
    public string? FiltroEstado { get; set; }
}

/// <summary>
/// ViewModel para el listado de pedidos del admin (HU16).
/// </summary>
public class AdminPedidosViewModel
{
    public List<Pedido> Pedidos { get; set; } = new();
    public string? FiltroEstado { get; set; }
    public string? FiltroBusqueda { get; set; }
    public string? FiltroFecha { get; set; }
}
