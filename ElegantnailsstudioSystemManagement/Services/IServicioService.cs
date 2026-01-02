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
        private readonly ApplicationDbContext _context;

        public ServicioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Servicio>> GetServiciosActivosAsync()
        {
            try
            {
                return await _context.Servicios
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
                return await _context.Servicios.FindAsync(id);
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
                _context.Servicios.Add(servicio);
                await _context.SaveChangesAsync();
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
                var existing = await _context.Servicios.FindAsync(servicio.Id);
                if (existing == null) return false;

                existing.Nombre = servicio.Nombre;
                existing.Descripcion = servicio.Descripcion;
                existing.Precio = servicio.Precio;
                existing.DuracionMinutos = servicio.DuracionMinutos;
                existing.ImagenUrl = servicio.ImagenUrl;
                existing.CategoriaId = servicio.CategoriaId;

                await _context.SaveChangesAsync();
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
                return await _context.Servicios
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
                Console.WriteLine($"🔍 [SERVICIO] Buscando servicio con ID: {id}");

                var servicio = await _context.Servicios.FindAsync(id);

                if (servicio == null)
                {
                    Console.WriteLine($"❌ [SERVICIO] Servicio {id} NO encontrado en la base de datos");
                    return false;
                }

                Console.WriteLine($"✅ [SERVICIO] Servicio encontrado: {servicio.Nombre} (ID: {servicio.Id})");

                
                var tieneCitas = await _context.Citas
                    .AnyAsync(c => c.ServicioId == id);

                Console.WriteLine($"🔍 [SERVICIO] Tiene citas asociadas: {tieneCitas}");

                if (tieneCitas)
                {
                    Console.WriteLine($"⚠️ [SERVICIO] NO se puede eliminar - Tiene citas asociadas");

                   
                    var cantidadCitas = await _context.Citas
                        .CountAsync(c => c.ServicioId == id);
                    Console.WriteLine($"📊 [SERVICIO] Cantidad de citas: {cantidadCitas}");

                    return false;
                }

                Console.WriteLine($"🗑️ [SERVICIO] Eliminando servicio: {servicio.Nombre}");

                _context.Servicios.Remove(servicio);
                int resultado = await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [SERVICIO] SaveChanges resultó en {resultado} filas afectadas");

                return resultado > 0;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"💥 [SERVICIO] ERROR de base de datos: {dbEx.Message}");
                if (dbEx.InnerException != null)
                    Console.WriteLine($"💥 [SERVICIO] ERROR interno: {dbEx.InnerException.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 [SERVICIO] ERROR general: {ex.Message}");
                Console.WriteLine($"💥 [SERVICIO] StackTrace: {ex.StackTrace}");
                return false;
            }
        }
    }
}






