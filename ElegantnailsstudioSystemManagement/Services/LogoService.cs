using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ElegantnailsstudioSystemManagement.Services
{
    public interface ILogoService
    {
        Task<string> GuardarLogoAsync(Stream fileStream, string fileName);
        bool EliminarLogo();
        string ObtenerLogoActual();
        string ObtenerRutaLogo();
      
    }

    public class LogoService : ILogoService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LogoService> _logger;
        private const string LogoFileName = "logo_custom.jpg";
        private const string LogoFolder = "uploads/logos";
        

        public LogoService(IWebHostEnvironment environment, ILogger<LogoService> logger)
        {
            _environment = environment;
            _logger = logger;

            
            var uploadsFolder = Path.Combine(_environment.WebRootPath, LogoFolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                _logger.LogInformation($"Carpeta de logos creada: {uploadsFolder}");
            }
        }

        public async Task<string> GuardarLogoAsync(Stream fileStream, string fileName)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, LogoFolder, LogoFileName);
                var relativePath = $"/{LogoFolder}/{LogoFileName}";

                _logger.LogInformation($"Guardando logo en: {filePath}");

                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Logo anterior eliminado: {filePath}");
                }

                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream);
                }

                _logger.LogInformation($"Logo guardado exitosamente: {relativePath}");

                return relativePath + "?v=" + DateTime.Now.Ticks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al guardar logo");
                throw;
            }
        }

        public bool EliminarLogo()
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, LogoFolder, LogoFileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Logo personalizado eliminado: {filePath}");
                    return true;
                }

                _logger.LogInformation($"No hay logo personalizado para eliminar");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar logo");
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
                   
                    return relativePath + "?t=" + DateTime.Now.Ticks;
                }

            
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logo actual");
                return "";
            }
        }

        public string ObtenerRutaLogo()
        {
            return Path.Combine(_environment.WebRootPath, LogoFolder, LogoFileName);
        }

     
    }
}









