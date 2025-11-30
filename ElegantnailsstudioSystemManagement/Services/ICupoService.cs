using ElegantNailsStudioSystemManagement.Models;

namespace ElegantNailsStudioSystemManagement.Services
{
    public interface ICupoService
    {
        Task<Cupo?> GetCupoAsync(DateTime fecha, string turno);
        Task<List<Cupo>> GetCuposPorFechaAsync(DateTime fecha);
        Task<bool> CheckDisponibilidadAsync(DateTime fecha, string turno);
        Task<bool> ReservarCupoAsync(DateTime fecha, string turno);
        Task<bool> LiberarCupoAsync(DateTime fecha, string turno);
        Task<Dictionary<string, bool>> GetDisponibilidadDiariaAsync(DateTime fecha);
    }

    public class CupoService : ICupoService
    {
        private readonly List<Cupo> _cupos = new();
        private int _nextId = 1;

        public Task<Cupo?> GetCupoAsync(DateTime fecha, string turno)
        {
            var cupo = _cupos.FirstOrDefault(c => c.Fecha.Date == fecha.Date && c.Turno == turno);
            if (cupo == null)
            {
                cupo = new Cupo
                {
                    Id = _nextId++,
                    Fecha = fecha.Date,
                    Turno = turno,
                    CupoMaximo = 5,
                    CupoActual = 0
                };
                _cupos.Add(cupo);
            }
            return Task.FromResult(cupo);
        }

        public Task<List<Cupo>> GetCuposPorFechaAsync(DateTime fecha)
        {
            var cupos = _cupos.Where(c => c.Fecha.Date == fecha.Date).ToList();

            if (!cupos.Any(c => c.Turno == "mañana"))
            {
                cupos.Add(new Cupo
                {
                    Id = _nextId++,
                    Fecha = fecha.Date,
                    Turno = "mañana",
                    CupoMaximo = 5,
                    CupoActual = 0
                });
            }
            if (!cupos.Any(c => c.Turno == "tarde"))
            {
                cupos.Add(new Cupo
                {
                    Id = _nextId++,
                    Fecha = fecha.Date,
                    Turno = "tarde",
                    CupoMaximo = 5,
                    CupoActual = 0
                });
            }

            return Task.FromResult(cupos);
        }

        public Task<bool> CheckDisponibilidadAsync(DateTime fecha, string turno)
        {
            var cupo = GetCupoAsync(fecha, turno).Result;
            return Task.FromResult(cupo?.TieneCupoDisponible ?? true);
        }

        public Task<bool> ReservarCupoAsync(DateTime fecha, string turno)
        {
            var cupo = GetCupoAsync(fecha, turno).Result;
            if (cupo != null && cupo.TieneCupoDisponible)
            {
                cupo.CupoActual++;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> LiberarCupoAsync(DateTime fecha, string turno)
        {
            var cupo = GetCupoAsync(fecha, turno).Result;
            if (cupo != null && cupo.CupoActual > 0)
            {
                cupo.CupoActual--;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<Dictionary<string, bool>> GetDisponibilidadDiariaAsync(DateTime fecha)
        {
            var cupoManana = GetCupoAsync(fecha, "mañana").Result;
            var cupoTarde = GetCupoAsync(fecha, "tarde").Result;

            var disponibilidad = new Dictionary<string, bool>
            {
                { "mañana", cupoManana?.TieneCupoDisponible ?? true },
                { "tarde", cupoTarde?.TieneCupoDisponible ?? true }
            };

            return Task.FromResult(disponibilidad);
        }
    }
}