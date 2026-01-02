using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace ElegantnailsstudioSystemManagement.Services
{
    public interface ILogoService
    {
        Task<string> GuardarLogoAsync(IFormFile archivo); // Cambia esto
        bool EliminarLogo();
        string ObtenerLogoActual();
        string ObtenerRutaLogo();
    }

    public class LogoService : ILogoService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LogoService> _logger;
        private const string LogoFolder = "uploads/logos";

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

        // NUEVO MÉTODO que recibe IFormFile directamente
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

                if (archivo.Length > 5 * 1024 * 1024)
                    return "";

                // Carpeta
                var carpetaLogos = Path.Combine(_environment.WebRootPath, LogoFolder);
                if (!Directory.Exists(carpetaLogos))
                    Directory.CreateDirectory(carpetaLogos);

                // Ruta fija
                var rutaCompleta = Path.Combine(carpetaLogos, "logo_custom.jpg");
                var rutaRelativa = $"/{LogoFolder}/logo_custom.jpg";

                // Guardar usando el archivo recibido
                using var stream = new FileStream(rutaCompleta, FileMode.Create);
                await archivo.CopyToAsync(stream);

                _logger.LogInformation($"✅ Logo guardado: {rutaRelativa}");
                return rutaRelativa;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando logo");
                return "";
            }
        }

        // Mantén el método viejo para compatibilidad
        public async Task<string> GuardarLogoAsync(Stream fileStream, string fileName)
        {
            // Si alguien llama al método viejo, lo convertimos
            var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            ms.Position = 0;

            // Crear un IFormFile simulado
            var formFile = new FormFile(ms, 0, ms.Length, "file", fileName);
            return await GuardarLogoAsync(formFile);
        }

        // Resto de métodos igual...
        public bool EliminarLogo()
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, LogoFolder, "logo_custom.jpg");
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
                var filePath = Path.Combine(_environment.WebRootPath, LogoFolder, "logo_custom.jpg");
                return File.Exists(filePath)
                    ? $"/{LogoFolder}/logo_custom.jpg?t={DateTime.Now.Ticks}"
                    : "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo logo");
                return "";
            }
        }

        public string ObtenerRutaLogo()
        {
            return Path.Combine(_environment.WebRootPath, LogoFolder, "logo_custom.jpg");
        }
    }
}