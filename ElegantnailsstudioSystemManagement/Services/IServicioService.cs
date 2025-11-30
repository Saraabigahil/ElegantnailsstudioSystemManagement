using ElegantNailsStudioSystemManagement.Services;
using ElegantNailsStudioSystemManagement.Models;

namespace ElegantNailsStudioSystemManagement.Services
{
    public interface IServicioService
    {
        Task<List<Servicio>> GetServiciosAsync();
        Task<List<Servicio>> GetServiciosActivosAsync();
        Task<List<Servicio>> GetServiciosByCategoriaAsync(int categoriaId);
        Task<List<Servicio>> GetServiciosByCategoriaNombreAsync(string categoriaNombre);
        Task<Servicio?> GetServicioByIdAsync(int id);
        Task<bool> CreateServicioAsync(Servicio servicio);
        Task<bool> UpdateServicioAsync(Servicio servicio);
        Task<bool> DeleteServicioAsync(int id);
        Task<bool> ToggleServicioStatusAsync(int id);
    }

    public class ServicioService : IServicioService
    {
        private readonly List<Servicio> _servicios = new();
        private readonly ICategoriaService _categoriaService;
        private int _nextId = 1;

        public ServicioService(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
            InitializeSampleData();
        }

        private async void InitializeSampleData()
        {
            var categorias = await _categoriaService.GetCategoriasAsync();
            var categoriaUñas = categorias.First(c => c.Nombre == "Uñas");
            var categoriaPestañas = categorias.First(c => c.Nombre == "Pestañas");

            // Servicios de Uñas
            _servicios.AddRange(new[]
            {
                new Servicio {
                    Id = _nextId++,
                    Nombre = "Uñas Postizas Largas a Presión",
                    Descripcion = "Uñas postizas largas aplicadas con técnica de presión profesional",
                    Precio = 30.00m,
                    DuracionMinutos = 120,
                    CategoriaId = categoriaUñas.Id,
                    ImagenUrl = "/images/unas-largas.jpg",
                    Activo = true
                },
                new Servicio {
                    Id = _nextId++,
                    Nombre = "Uñas Postizas Cortas Francesas",
                    Descripcion = "Uñas postizas cortas con diseño francés clásico elegante",
                    Precio = 20.00m,
                    DuracionMinutos = 90,
                    CategoriaId = categoriaUñas.Id,
                    ImagenUrl = "/images/unas-francesas.jpg",
                    Activo = true
                }
            });

            // Servicios de Pestañas
            _servicios.AddRange(new[]
            {
                new Servicio {
                    Id = _nextId++,
                    Nombre = "Extensiones Efecto Rímel",
                    Descripcion = "Extensiones que crean efecto de rímel voluminoso y dramático",
                    Precio = 45.00m,
                    DuracionMinutos = 120,
                    CategoriaId = categoriaPestañas.Id,
                    ImagenUrl = "/images/rimel.jpg",
                    Activo = true
                },
                new Servicio {
                    Id = _nextId++,
                    Nombre = "Extensiones 6D Multidimensional",
                    Descripcion = "Extensiones con efecto multidimensional 6D de lujo",
                    Precio = 65.00m,
                    DuracionMinutos = 180,
                    CategoriaId = categoriaPestañas.Id,
                    ImagenUrl = "/images/6d.jpg",
                    Activo = true
                }
            });
        }

        public Task<List<Servicio>> GetServiciosAsync()
        {
            return Task.FromResult(_servicios);
        }

        public Task<List<Servicio>> GetServiciosActivosAsync()
        {
            return Task.FromResult(_servicios.Where(s => s.Activo).ToList());
        }

        public Task<List<Servicio>> GetServiciosByCategoriaAsync(int categoriaId)
        {
            var servicios = _servicios
                .Where(s => s.CategoriaId == categoriaId && s.Activo)
                .ToList();
            return Task.FromResult(servicios);
        }

        public async Task<List<Servicio>> GetServiciosByCategoriaNombreAsync(string categoriaNombre)
        {
            var categoria = await _categoriaService.GetCategoriaByNombreAsync(categoriaNombre);
            if (categoria == null) return new List<Servicio>();

            return await GetServiciosByCategoriaAsync(categoria.Id);
        }

        public Task<Servicio?> GetServicioByIdAsync(int id)
        {
            var servicio = _servicios.FirstOrDefault(s => s.Id == id);
            return Task.FromResult(servicio);
        }

        public Task<bool> CreateServicioAsync(Servicio servicio)
        {
            servicio.Id = _nextId++;
            servicio.Activo = true;
            _servicios.Add(servicio);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateServicioAsync(Servicio servicio)
        {
            var existing = _servicios.FirstOrDefault(s => s.Id == servicio.Id);
            if (existing != null)
            {
                existing.Nombre = servicio.Nombre;
                existing.Descripcion = servicio.Descripcion;
                existing.Precio = servicio.Precio;
                existing.DuracionMinutos = servicio.DuracionMinutos;
                existing.CategoriaId = servicio.CategoriaId;
                existing.ImagenUrl = servicio.ImagenUrl;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteServicioAsync(int id)
        {
            var servicio = _servicios.FirstOrDefault(s => s.Id == id);
            if (servicio != null)
            {
                servicio.Activo = false;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> ToggleServicioStatusAsync(int id)
        {
            var servicio = _servicios.FirstOrDefault(s => s.Id == id);
            if (servicio != null)
            {
                servicio.Activo = !servicio.Activo;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
