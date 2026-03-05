using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PryCafeteria.Services
{
    /// <summary>
    /// Implementación del servicio de imágenes usando SixLabors.ImageSharp.
    /// HU-Productos: E1 (subida), E4 (reemplazo), E9 (validación), E10 (redimensionado 800x800)
    /// </summary>
    public class ImagenService : IImagenService
    {
        private readonly IWebHostEnvironment _env;

        // Formatos permitidos: HU-Productos E9
        private static readonly string[] _extensionesPermitidas = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] _contentTypesPermitidos =
            { "image/jpeg", "image/jpg", "image/png" };

        // Tamaño máximo: 2 MB — HU-Productos E9
        private const long MaxBytes = 2 * 1024 * 1024;

        // Resolución destino: HU-Productos E10
        private const int TargetSize = 800;

        public ImagenService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<(string? ruta, string? error)> GuardarAsync(
            IFormFile archivo,
            string categoriaNombre)
        {
            // ── Validación de extensión ──────────────────────────────────────
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!_extensionesPermitidas.Contains(extension))
                return (null, "Solo se permiten imágenes .jpg, .jpeg y .png");

            if (!_contentTypesPermitidos.Contains(archivo.ContentType.ToLowerInvariant()))
                return (null, "El tipo de archivo no es una imagen válida");

            // ── Validación de tamaño ─────────────────────────────────────────
            if (archivo.Length > MaxBytes)
                return (null, "La imagen no puede superar 2 MB");

            // ── Preparar carpeta destino ─────────────────────────────────────
            // Sanitizar nombre de categoría para usar como nombre de carpeta
            var carpeta = SanitizarNombreCarpeta(categoriaNombre);
            var dirFisico = Path.Combine(
                _env.WebRootPath, "images", "productos", carpeta);

            if (!Directory.Exists(dirFisico))
                Directory.CreateDirectory(dirFisico);

            // ── Nombre de archivo único ──────────────────────────────────────
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(dirFisico, nombreArchivo);

            // ── Redimensionar a 800x800 con ImageSharp ──────────────────────
            using var stream = archivo.OpenReadStream();
            using var imagen = await Image.LoadAsync(stream);

            // Resize con padding para mantener aspecto sin deformación
            imagen.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(TargetSize, TargetSize),
                Mode = ResizeMode.Pad   // rellena con fondo transparente/blanco si es necesario
            }));

            await imagen.SaveAsync(rutaFisica);

            // ── Ruta relativa para almacenar en BD ──────────────────────────
            var rutaRelativa = $"/images/productos/{carpeta}/{nombreArchivo}";
            return (rutaRelativa, null);
        }

        public void Eliminar(string? rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa)) return;

            // Convertir ruta relativa (/images/...) a ruta física
            var rutaFisica = Path.Combine(
                _env.WebRootPath,
                rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(rutaFisica))
                File.Delete(rutaFisica);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Convierte "Bebidas Calientes" → "Bebidas_Calientes" para usar como nombre de carpeta.
        /// </summary>
        private static string SanitizarNombreCarpeta(string nombre)
        {
            // Reemplaza espacios por guión bajo y elimina caracteres no válidos en rutas
            var sanitizado = string.Concat(
                nombre.Normalize()
                      .Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_'))
                .Trim()
                .Replace(' ', '_');

            return string.IsNullOrEmpty(sanitizado) ? "General" : sanitizado;
        }
    }
}
