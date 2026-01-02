using ElegantnailsstudioSystemManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ElegantnailsstudioSystemManagement.Services
{
    public interface ICupoService
    {
        Task<bool> CheckDisponibilidadAsync(DateTime fecha, string turno, int duracionRequerida);
        Task<bool> ReservarCupoAsync(DateTime fecha, string turno);
        Task<bool> LiberarCupoAsync(DateTime fecha, string turno);
        Task<Cupo?> GetCupoByFechaTurnoAsync(DateTime fecha, string turno);
        Task<List<Cupo>> GetCuposByFechaAsync(DateTime fecha);
        Task<bool> HayCupoDisponibleAsync(DateTime fechaCita, string turno);
        Task<bool> HabilitarTurnoAsync(DateTime fecha, string turno, int cuposMaximos);
        Task<List<Cupo>> GetCuposHabilitadosAsync(DateTime desde, DateTime hasta);
    }

    public class CupoService : ICupoService
    {
        private readonly ApplicationDbContext _context;

        public CupoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Crear o actualizar cupos 
        public async Task<bool> HabilitarTurnoAsync(DateTime fecha, string turno, int cuposMaximos)
        {
            try
            {
                var cupoExistente = await GetCupoByFechaTurnoAsync(fecha, turno);

                if (cupoExistente != null)
                {
                    // Actualizar
                    cupoExistente.CupoMaximo = cuposMaximos;
                    cupoExistente.Habilitado = true;
                }
                else
                {
                    // Crear nuevo
                    var nuevoCupo = new Cupo
                    {
                        Fecha = fecha.Date, 
                        Turno = turno,
                        CupoMaximo = cuposMaximos,
                        CupoReservado = 0,
                        Habilitado = true,
                        FechaHabilitacion = DateTime.Now
                    };
                    _context.Cupos.Add(nuevoCupo);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR HabilitarTurnoAsync: {ex.Message}");
                return false;
            }
        }

        // verificar disponibilidad
        public async Task<bool> CheckDisponibilidadAsync(DateTime fecha, string turno, int duracionRequerida)
        {
            try
            {
                var cupo = await GetCupoByFechaTurnoAsync(fecha, turno);

                if (cupo == null || !cupo.Habilitado)
                {
                    Console.WriteLine($"❌ Turno no habilitado para {fecha:dd/MM/yyyy} - {turno}");
                    return false;
                }

               
                bool disponible = cupo.CuposDisponibles > 0;

                Console.WriteLine($"📊 Fecha: {fecha:dd/MM/yyyy} - Turno: {turno}");
                Console.WriteLine($"📊 Cupos disponibles: {cupo.CuposDisponibles}/{cupo.CupoMaximo}");
                Console.WriteLine($"📊 Duración requerida: {duracionRequerida} minutos");
                Console.WriteLine($"📊 ¿Disponible? {disponible}");

                return disponible;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR CheckDisponibilidadAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ReservarCupoAsync(DateTime fecha, string turno)
        {
            try
            {
                var cupo = await GetCupoByFechaTurnoAsync(fecha, turno);

                if (cupo == null || !cupo.Habilitado || cupo.CuposDisponibles <= 0)
                {
                    Console.WriteLine($"❌ No se puede reservar: {fecha:dd/MM/yyyy} - {turno}");
                    Console.WriteLine($"❌ Cupo existe: {cupo != null}");
                    Console.WriteLine($"❌ Habilitado: {cupo?.Habilitado}");
                    Console.WriteLine($"❌ Cupos disponibles: {cupo?.CuposDisponibles}");
                    return false;
                }

                cupo.CupoReservado++;
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Cupo reservado: {fecha:dd/MM/yyyy} - {turno}");
                Console.WriteLine($"✅ Nuevo cupo reservado: {cupo.CupoReservado}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR ReservarCupoAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LiberarCupoAsync(DateTime fecha, string turno)
        {
            try
            {
                var cupo = await GetCupoByFechaTurnoAsync(fecha, turno);

                if (cupo == null || cupo.CupoReservado <= 0)
                {
                    Console.WriteLine($"❌ No se puede liberar cupo: {fecha:dd/MM/yyyy} - {turno}");
                    return false;
                }

                cupo.CupoReservado--;
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Cupo liberado: {fecha:dd/MM/yyyy} - {turno}");
                Console.WriteLine($"✅ Nuevo cupo reservado: {cupo.CupoReservado}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR LiberarCupoAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<Cupo?> GetCupoByFechaTurnoAsync(DateTime fecha, string turno)
        {
            return await _context.Cupos
                .FirstOrDefaultAsync(c => c.Fecha.Date == fecha.Date && c.Turno == turno);
        }

        public async Task<List<Cupo>> GetCuposByFechaAsync(DateTime fecha)
        {
            return await _context.Cupos
                .Where(c => c.Fecha.Date == fecha.Date)
                .OrderBy(c => c.Turno)
                .ToListAsync();
        }

       
        public async Task<List<Cupo>> GetCuposHabilitadosAsync(DateTime desde, DateTime hasta)
        {
            return await _context.Cupos
                .Where(c => c.Fecha.Date >= desde.Date &&
                           c.Fecha.Date <= hasta.Date &&
                           c.Habilitado)
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Turno)
                .ToListAsync();
        }

       
        public async Task<bool> HayCupoDisponibleAsync(DateTime fechaCita, string turno)
        {
            try
            {
                var cupo = await GetCupoByFechaTurnoAsync(fechaCita, turno);

                if (cupo == null || !cupo.Habilitado)
                {
                    return false;
                }

                return cupo.CuposDisponibles > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR HayCupoDisponibleAsync: {ex.Message}");
                return false;
            }
        }
    }
}






