using Microsoft.AspNetCore.Http;

namespace PryCafeteria.Services
{
    /// <summary>
    /// Contrato del servicio de gestión de imágenes de productos.
    /// HU-Productos: E1, E2, E4, E9, E10
    /// </summary>
    public interface IImagenService
    {
        /// <summary>
        /// Valida, redimensiona a 800x800 px y guarda la imagen en
        /// /wwwroot/images/productos/{categoriaNombre}/
        /// </summary>
        /// <returns>Ruta relativa almacenada, o null si el archivo es inválido.</returns>
        Task<(string? ruta, string? error)> GuardarAsync(
            IFormFile archivo,
            string categoriaNombre);

        /// <summary>Elimina el archivo físico si existe en wwwroot.</summary>
        void Eliminar(string? rutaRelativa);
    }
}
