using ElegantnailsstudioSystemManagement.Models;

namespace ElegantnailsstudioSystemManagement.Services
{
    public interface ICupoService
    {
        Task<bool> CheckDisponibilidadAsync(DateTime fecha, string turno);
        Task<bool> ReservarCupoAsync(DateTime fecha, string turno);
        Task<bool> LiberarCupoAsync(DateTime fecha, string turno);
    }

    public class CupoService : ICupoService
    {
        private readonly List<Cupo> _cupos = new();

        public Task<bool> CheckDisponibilidadAsync(DateTime fecha, string turno)
        {
            var cupo = _cupos.FirstOrDefault(c => c.Fecha.Date == fecha.Date && c.Turno == turno);
            if (cupo == null)
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(cupo.CupoActual < cupo.CupoMaximo && cupo.Disponible);
        }

        public Task<bool> ReservarCupoAsync(DateTime fecha, string turno)
        {
            var cupo = _cupos.FirstOrDefault(c => c.Fecha.Date == fecha.Date && c.Turno == turno);
            if (cupo == null)
            {
                cupo = new Cupo
                {
                    Id = _cupos.Count + 1,
                    Fecha = fecha.Date,
                    Turno = turno,
                    CupoActual = 1,
                    CupoMaximo = 5,
                    Disponible = true
                };
                _cupos.Add(cupo);
                return Task.FromResult(true);
            }
            else if (cupo.CupoActual < cupo.CupoMaximo && cupo.Disponible)
            {
                cupo.CupoActual++;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> LiberarCupoAsync(DateTime fecha, string turno)
        {
            var cupo = _cupos.FirstOrDefault(c => c.Fecha.Date == fecha.Date && c.Turno == turno);
            if (cupo != null && cupo.CupoActual > 0)
            {
                cupo.CupoActual--;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}