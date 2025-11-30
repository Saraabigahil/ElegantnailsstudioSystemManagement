using ElegantnailsstudioSystemManagement.Models;

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
        Task<bool> CancelarCitaAsync(int id);
        Task<bool> CompletarCitaAsync(int id);
        Task<List<Cita>> GetCitasByFechaAsync(DateTime fecha);
        Task<List<Cita>> GetCitasByClienteAsync(int clienteId);
        Task<List<Cita>> GetCitasByEstadoAsync(string estado);
    }

    public class CitaService : ICitaService
    {
        private readonly List<Cita> _citas = new();
        private readonly ICupoService _cupoService;
        private int _nextId = 1;

        public CitaService(ICupoService cupoService)
        {
            _cupoService = cupoService;
        }

        public Task<List<Cita>> GetCitasAsync()
        {
            return Task.FromResult(_citas.ToList());
        }

        public Task<List<Cita>> GetCitasActivasAsync()
        {
            var citasActivas = _citas
                .Where(c => c.Estado != "cancelada" && c.Estado != "completada")
                .ToList();
            return Task.FromResult(citasActivas);
        }

        public Task<Cita?> GetCitaByIdAsync(int id)
        {
            return Task.FromResult(_citas.FirstOrDefault(c => c.Id == id));
        }

        public async Task<bool> CreateCitaAsync(Cita cita)
        {
            var disponible = await _cupoService.CheckDisponibilidadAsync(cita.FechaCita, cita.Turno);
            if (!disponible)
                return false;

            var cupoReservado = await _cupoService.ReservarCupoAsync(cita.FechaCita, cita.Turno);
            if (!cupoReservado)
                return false;

            cita.Id = _nextId++;
            cita.Estado = "pendiente";
            _citas.Add(cita);
            return true;
        }

        public Task<bool> UpdateCitaAsync(Cita cita)
        {
            var existing = _citas.FirstOrDefault(c => c.Id == cita.Id);
            if (existing != null)
            {
                existing.ClienteId = cita.ClienteId;
                existing.ServicioId = cita.ServicioId;
                existing.FechaCita = cita.FechaCita;
                existing.Turno = cita.Turno;
                existing.Estado = cita.Estado;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public async Task<bool> DeleteCitaAsync(int id)
        {
            var cita = _citas.FirstOrDefault(c => c.Id == id);
            if (cita != null)
            {
                await _cupoService.LiberarCupoAsync(cita.FechaCita, cita.Turno);
                _citas.Remove(cita);
                return true;
            }
            return false;
        }

        public async Task<bool> CancelarCitaAsync(int id)
        {
            var cita = _citas.FirstOrDefault(c => c.Id == id);
            if (cita != null)
            {
                cita.Estado = "cancelada";
                await _cupoService.LiberarCupoAsync(cita.FechaCita, cita.Turno);
                return true;
            }
            return false;
        }

        public Task<bool> CompletarCitaAsync(int id)
        {
            var cita = _citas.FirstOrDefault(c => c.Id == id);
            if (cita != null)
            {
                cita.Estado = "completada";
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<List<Cita>> GetCitasByFechaAsync(DateTime fecha)
        {
            var citas = _citas
                .Where(c => c.FechaCita.Date == fecha.Date)
                .OrderBy(c => c.Turno)
                .ToList();
            return Task.FromResult(citas);
        }

        public Task<List<Cita>> GetCitasByClienteAsync(int clienteId)
        {
            var citas = _citas
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaCita)
                .ToList();
            return Task.FromResult(citas);
        }

        public Task<List<Cita>> GetCitasByEstadoAsync(string estado)
        {
            var citas = _citas
                .Where(c => c.Estado == estado)
                .OrderBy(c => c.FechaCita)
                .ThenBy(c => c.Turno)
                .ToList();
            return Task.FromResult(citas);
        }
    }
}