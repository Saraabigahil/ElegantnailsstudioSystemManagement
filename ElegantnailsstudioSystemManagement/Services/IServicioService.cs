using ElegantnailsstudioSystemManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ElegantnailsstudioSystemManagement.Services
{
    public interface IServicioService
    {
        Task<List<Servicio>> GetServiciosActivosAsync();
        Task<Servicio?> GetServicioByIdAsync(int id);
        Task<bool> CrearServicio(Servicio servicio);
        Task<bool> ActualizarServicio(Servicio servicio);
        Task<bool> EliminarServicioAsync(int id);
        Task<List<Servicio>> GetServiciosByCategoriaAsync(int categoriaId);
    }

    public class ServicioService : IServicioService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ServicioService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Servicio>> GetServiciosActivosAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                return await context.Servicios
                    .Include(s => s.Categoria)
                    .OrderBy(s => s.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetServiciosActivosAsync: {ex.Message}");
                return new List<Servicio>();
            }
        }

        public async Task<Servicio?> GetServicioByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Servicios.FindAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetServicioByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CrearServicio(Servicio servicio)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                context.Servicios.Add(servicio);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR CrearServicio: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarServicio(Servicio servicio)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var existente = await context.Servicios.FindAsync(servicio.Id);
                if (existente == null) return false;

                existente.Nombre = servicio.Nombre;
                existente.Descripcion = servicio.Descripcion;
                existente.Precio = servicio.Precio;
                existente.DuracionMinutos = servicio.DuracionMinutos;
                existente.ImagenUrl = servicio.ImagenUrl;
                existente.CategoriaId = servicio.CategoriaId;

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR ActualizarServicio: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Servicio>> GetServiciosByCategoriaAsync(int categoriaId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                return await context.Servicios
                    .Where(s => s.CategoriaId == categoriaId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetServiciosByCategoriaAsync: {ex.Message}");
                return new List<Servicio>();
            }
        }

        public async Task<bool> EliminarServicioAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var servicio = await context.Servicios.FindAsync(id);
                if (servicio == null) return false;

                
                var tieneCitasActivas = await context.Citas
                    .AnyAsync(c => c.ServicioId == id && c.Estado != "cancelada");

                if (tieneCitasActivas)
                {
                    Console.WriteLine($"⚠️ No se puede eliminar servicio {id}: tiene citas activas");
                    return false;
                }

                context.Servicios.Remove(servicio);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR EliminarServicioAsync: {ex.Message}");
                return false;
            }
        }
    }
}
