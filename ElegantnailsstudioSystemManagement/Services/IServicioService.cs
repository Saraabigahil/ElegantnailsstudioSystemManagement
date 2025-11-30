using ElegantnailsstudioSystemManagement.Models;

namespace ElegantnailsstudioSystemManagement.Services
{
    public interface IServicioService
    {
        Task<List<Servicio>> GetServiciosActivosAsync();
        Task<Servicio?> GetServicioByIdAsync(int id);
        Task<bool> CrearServicio(Servicio servicio);
        Task<bool> ActualizarServicio(Servicio servicio);
        Task<bool> EliminarServicio(int id);
    }

    public class ServicioService : IServicioService
    {
        private readonly List<Servicio> _servicios = new();
        private int _nextId = 1;

        public ServicioService()
        {
            _servicios.AddRange(new[]
            {
                new Servicio
                {
                    Id = _nextId++,
                    Nombre = "Uñas Postizas largas a presión",
                    Descripcion = "Servicio de uñas postizas",
                    Precio = 30.00m,
                    DuracionMinutos = 120,
                    ImagenUrl = "images/servicios/unas-largas.jpg",
                    CategoriaId = 1
                },
                new Servicio
                {
                    Id = _nextId++,
                    Nombre = "Extensiones efecto rímel",
                    Descripcion = "Extensiones con efecto rímel",
                    Precio = 45.00m,
                    DuracionMinutos = 120,
                    ImagenUrl = "images/servicios/extensiones-rimel.jpg",
                    CategoriaId = 2
                },
                new Servicio
                {
                    Id = _nextId++,
                    Nombre = "Uñas postizas cortas francesas",
                    Descripcion = "Uñas postizas cortas estilo francés",
                    Precio = 20.00m,
                    DuracionMinutos = 90,
                    ImagenUrl = "images/servicios/unas-francesas.jpg",
                    CategoriaId = 1
                }
            });
        }

        public Task<List<Servicio>> GetServiciosActivosAsync()
        {
            return Task.FromResult(_servicios.ToList());
        }

        public Task<Servicio?> GetServicioByIdAsync(int id)
        {
            return Task.FromResult(_servicios.FirstOrDefault(s => s.Id == id));
        }

        public Task<bool> CrearServicio(Servicio servicio)
        {
            servicio.Id = _nextId++;
            _servicios.Add(servicio);
            return Task.FromResult(true);
        }

        public Task<bool> ActualizarServicio(Servicio servicio)
        {
            var existing = _servicios.FirstOrDefault(s => s.Id == servicio.Id);
            if (existing != null)
            {
                existing.Nombre = servicio.Nombre;
                existing.Descripcion = servicio.Descripcion;
                existing.Precio = servicio.Precio;
                existing.DuracionMinutos = servicio.DuracionMinutos;
                existing.ImagenUrl = servicio.ImagenUrl;
                existing.CategoriaId = servicio.CategoriaId;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> EliminarServicio(int id)
        {
            var servicio = _servicios.FirstOrDefault(s => s.Id == id);
            if (servicio != null)
            {
                _servicios.Remove(servicio);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}