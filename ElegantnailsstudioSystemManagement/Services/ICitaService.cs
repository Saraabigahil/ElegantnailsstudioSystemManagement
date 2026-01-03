using ElegantnailsstudioSystemManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ElegantnailsstudioSystemManagement.Services
{
    public interface ICitaService
    {
        Task<List<Cita>> GetCitasAsync();
        Task<List<Cita>> GetCitasActivasAsync();
        Task<Cita?> GetCitaByIdAsync(int id);
        Task<bool> CreateCitaAsync(Cita cita);
        Task<bool> UpdateCitaAsync(Cita cita);
        Task<bool> DeleteCitaAsync(int id);
        Task<bool> CancelarCitaAsync(int id, int clienteId);
        Task<bool> CompletarCitaAsync(int id);
        Task<bool> CancelarCitaAdminAsync(int citaId);

        Task<List<Cita>> GetCitasByFechaAsync(DateTime fecha);
        Task<List<Cita>> GetCitasByClienteAsync(int clienteId);
        Task<List<Cita>> GetCitasByEstadoAsync(string estado);
    }

    public class CitaService : ICitaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICupoService _cupoService;

        public CitaService(ApplicationDbContext context, ICupoService cupoService)
        {
            _context = context;
            _cupoService = cupoService;
        }

        public async Task<bool> CreateCitaAsync(Cita cita)
        {
            try
            {
                var servicio = await _context.Servicios.FindAsync(cita.ServicioId);
                if (servicio == null)
                {
                    Console.WriteLine($"❌ Servicio con ID {cita.ServicioId} no encontrado");
                    return false;
                }

                // VALIDA que la fecha no sea pasada
                if (cita.FechaCita.Date < DateTime.Today)
                {
                    Console.WriteLine($"❌ No se pueden agendar citas en fechas pasadas: {cita.FechaCita.Date}");
                    return false;
                }

                // VALIDA que el turno no haya pasado
                var turnoPasado = await _cupoService.IsTurnoPasadoAsync(cita.FechaCita, cita.Turno);
                if (turnoPasado)
                {
                    Console.WriteLine($"❌ No se pueden agendar citas en turnos que ya pasaron: {cita.FechaCita.Date} - {cita.Turno}");
                    return false;
                }

                var cupo = await _cupoService.GetCupoByFechaTurnoAsync(cita.FechaCita, cita.Turno);
                if (cupo == null || !cupo.Habilitado)
                {
                    Console.WriteLine($"❌ Cupo no habilitado para {cita.FechaCita.Date} - Turno: {cita.Turno}");
                    return false;
                }

                var disponible = await _cupoService.CheckDisponibilidadAsync(
                    cita.FechaCita,
                    cita.Turno,
                    servicio.DuracionMinutos
                );

                if (!disponible)
                {
                    Console.WriteLine($"❌ No hay cupo disponible para {cita.FechaCita.Date} - {cita.Turno}");
                    return false;
                }

                var cupoReservado = await _cupoService.ReservarCupoAsync(cita.FechaCita, cita.Turno);
                if (!cupoReservado)
                {
                    Console.WriteLine($"❌ No se pudo reservar el cupo");
                    return false;
                }

                cita.Estado = "pendiente";
                cita.FechaCreacion = DateTime.Now;
                _context.Citas.Add(cita);

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Cita creada exitosamente - ID: {cita.Id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR CreateCitaAsync: {ex.Message}");
                Console.WriteLine($"💥 StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> CancelarCitaAsync(int citaId, int clienteId)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(citaId);
                if (cita == null) return false;

                if (cita.ClienteId != clienteId)
                    return false;

                if (cita.Estado != "pendiente") return false;

                await _cupoService.LiberarCupoAsync(cita.FechaCita, cita.Turno);

                cita.Estado = "cancelada";
                cita.FechaCancelacion = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR CancelarCitaAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Cita>> GetCitasAsync()
        {
            try
            {
                return await _context.Citas
                    .Include(c => c.Cliente)
                    .Include(c => c.Servicio)
                    .OrderByDescending(c => c.FechaCita)
                    .ThenBy(c => c.Turno)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetCitasAsync: {ex.Message}");
                return new List<Cita>();
            }
        }

        public async Task<List<Cita>> GetCitasActivasAsync()
        {
            try
            {
                return await _context.Citas
                    .Include(c => c.Cliente)
                    .Include(c => c.Servicio)
                    .Where(c => c.Estado != "cancelada" && c.Estado != "completada")
                    .OrderBy(c => c.FechaCita)
                    .ThenBy(c => c.Turno)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetCitasActivasAsync: {ex.Message}");
                return new List<Cita>();
            }
        }

        public async Task<Cita?> GetCitaByIdAsync(int id)
        {
            try
            {
                return await _context.Citas.FindAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetCitaByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateCitaAsync(Cita cita)
        {
            try
            {
                var existing = await _context.Citas.FindAsync(cita.Id);
                if (existing == null) return false;

                var servicio = await _context.Servicios.FindAsync(cita.ServicioId);
                if (servicio == null) return false;

                // Si cambia la fecha o turno, verifica cupo
                if (existing.FechaCita.Date != cita.FechaCita.Date || existing.Turno != cita.Turno)
                {
                    // Valida que el nuevo turno no haya pasado
                    var turnoPasado = await _cupoService.IsTurnoPasadoAsync(cita.FechaCita, cita.Turno);
                    if (turnoPasado) return false;

                    var disponible = await _cupoService.CheckDisponibilidadAsync(
                        cita.FechaCita,
                        cita.Turno,
                        servicio.DuracionMinutos);

                    if (!disponible) return false;

                    await _cupoService.LiberarCupoAsync(existing.FechaCita, existing.Turno);

                    var cupoReservado = await _cupoService.ReservarCupoAsync(cita.FechaCita, cita.Turno);
                    if (!cupoReservado) return false;
                }

                existing.ClienteId = cita.ClienteId;
                existing.ServicioId = cita.ServicioId;
                existing.FechaCita = cita.FechaCita;
                existing.Turno = cita.Turno;
                existing.Estado = cita.Estado;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR UpdateCitaAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCitaAsync(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita == null) return false;

                // Libera cupo
                await _cupoService.LiberarCupoAsync(cita.FechaCita, cita.Turno);

                _context.Citas.Remove(cita);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR DeleteCitaAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CompletarCitaAsync(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita == null) return false;

                cita.Estado = "completada";
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR CompletarCitaAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Cita>> GetCitasByFechaAsync(DateTime fecha)
        {
            try
            {
                return await _context.Citas
                    .Where(c => c.FechaCita.Date == fecha.Date)
                    .OrderBy(c => c.Turno)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetCitasByFechaAsync: {ex.Message}");
                return new List<Cita>();
            }
        }

        public async Task<List<Cita>> GetCitasByClienteAsync(int clienteId)
        {
            try
            {
                return await _context.Citas
                    .Include(c => c.Servicio)
                    .Include(c => c.Cliente)
                    .Where(c => c.ClienteId == clienteId)
                    .OrderByDescending(c => c.FechaCita)
                    .ThenBy(c => c.Turno)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetCitasByClienteAsync: {ex.Message}");
                return new List<Cita>();
            }
        }

        public async Task<List<Cita>> GetCitasByEstadoAsync(string estado)
        {
            try
            {
                return await _context.Citas
                    .Where(c => c.Estado == estado)
                    .OrderBy(c => c.FechaCita)
                    .ThenBy(c => c.Turno)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetCitasByEstadoAsync: {ex.Message}");
                return new List<Cita>();
            }
        }

        public async Task<bool> CancelarCitaAdminAsync(int citaId)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(citaId);
                if (cita == null) return false;

                if (cita.Estado != "pendiente") return false;

                await _cupoService.LiberarCupoAsync(cita.FechaCita, cita.Turno);

                cita.Estado = "cancelada";
                cita.FechaCancelacion = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR CancelarCitaAdminAsync: {ex.Message}");
                return false;
            }
        }
    }
}