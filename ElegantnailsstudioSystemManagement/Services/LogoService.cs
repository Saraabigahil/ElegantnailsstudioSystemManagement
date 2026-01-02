using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace ElegantnailsstudioSystemManagement.Services
{
    public interface ILogoService
    {
        Task<string> GuardarLogoAsync(IFormFile archivo);
        bool EliminarLogo();
        string ObtenerLogoActual();
        string ObtenerRutaLogo();
    }

    public class LogoService : ILogoService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LogoService> _logger;
        private const string LogoFolder = "uploads/logos";
        private const string LogoFileName = "logo_custom.jpg"; // Agregado

        public LogoService(IWebHostEnvironment environment, ILogger<LogoService> logger)
        {
            _environment = environment;
            _logger = logger;

            // Crear carpeta
            var uploadsFolder = Path.Combine(_environment.WebRootPath, LogoFolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
        }

        public async Task<string> GuardarLogoAsync(IFormFile archivo)
        {
            try
            {
                // Validaciones
                if (archivo == null || archivo.Length == 0)
                    return "";

                var extensionesPermitidas = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
                var extension = Path.GetExtension(archivo.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension))
                    return "";

                if (archivo.Length > 5 * 1024 * 1024) // 5MB
                    return "";

                // Carpeta
                var carpetaLogos = Path.Combine(_environment.WebRootPath, LogoFolder);
                if (!Directory.Exists(carpetaLogos))
                    Directory.CreateDirectory(carpetaLogos);

                // Ruta completa
                var rutaCompleta = Path.Combine(carpetaLogos, LogoFileName);

                // Eliminar logo anterior si existe
                if (File.Exists(rutaCompleta))
                {
                    File.Delete(rutaCompleta);
                    _logger.LogInformation($"Logo anterior eliminado: {rutaCompleta}");
                }

                // Guardar nuevo logo
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                var rutaRelativa = $"/{LogoFolder}/{LogoFileName}";
                _logger.LogInformation($"✅ Logo guardado: {rutaRelativa}");
                return rutaRelativa;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando logo");
                return "";
            }
        }

        // Método para compatibilidad (opcional)
        public async Task<string> GuardarLogoAsync(Stream fileStream, string fileName)
        {
            var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            ms.Position = 0;

            var formFile = new FormFile(ms, 0, ms.Length, "file", fileName);
            return await GuardarLogoAsync(formFile);
        }

        public bool EliminarLogo()
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, LogoFolder, LogoFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando logo");
                return false;
            }
        }

        public string ObtenerLogoActual()
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, LogoFolder, LogoFileName);
                var relativePath = $"/{LogoFolder}/{LogoFileName}";

                if (File.Exists(filePath))
                {
                    // Agregar timestamp para evitar caché
                    return relativePath + "?t=" + DateTime.Now.Ticks;
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo logo");
                return "";
            }
        }

        public string ObtenerRutaLogo()
        {
            return Path.Combine(_environment.WebRootPath, LogoFolder, LogoFileName);
        }
    }
}