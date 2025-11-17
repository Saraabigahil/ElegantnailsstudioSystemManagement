using ElegantNailsStudioSystemManagement.Models;

namespace ElegantNailsStudioSystemManagement.Services
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
        private readonly IUsuarioService _usuarioService;
        private readonly IServicioService _servicioService;
        private readonly ICupoService _cupoService;
        private int _nextId = 1;

        public CitaService(IUsuarioService usuarioService, IServicioService servicioService, ICupoService cupoService)
        {
            _usuarioService = usuarioService;
            _servicioService = servicioService;
            _cupoService = cupoService;
        }

        public async Task<List<Cita>> GetCitasAsync()
        {
            await LoadNavigationProperties(_citas);
            return _citas;
        }

        public async Task<List<Cita>> GetCitasActivasAsync()
        {
            var citasActivas = _citas
                .Where(c => c.Estado != "cancelada" && c.Estado != "completada")
                .ToList();

            await LoadNavigationProperties(citasActivas);
            return citasActivas;
        }

        public async Task<Cita?> GetCitaByIdAsync(int id)
        {
            var cita = _citas.FirstOrDefault(c => c.Id == id);
            if (cita != null)
            {
                await LoadNavigationProperties(new List<Cita> { cita });
            }
            return cita;
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
            cita.FechaCreacion = DateTime.Now;
            cita.Estado = "pendiente";
            _citas.Add(cita);

            await LoadNavigationProperties(new List<Cita> { cita });
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
                existing.Notas = cita.Notas;
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

        public async Task<List<Cita>> GetCitasByFechaAsync(DateTime fecha)
        {
            var citas = _citas
                .Where(c => c.FechaCita.Date == fecha.Date)
                .OrderBy(c => c.Turno)
                .ToList();

            await LoadNavigationProperties(citas);
            return citas;
        }

        public async Task<List<Cita>> GetCitasByClienteAsync(int clienteId)
        {
            var citas = _citas
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaCita)
                .ThenByDescending(c => c.FechaCreacion)
                .ToList();

            await LoadNavigationProperties(citas);
            return citas;
        }

        public async Task<List<Cita>> GetCitasByEstadoAsync(string estado)
        {
            var citas = _citas
                .Where(c => c.Estado == estado)
                .OrderBy(c => c.FechaCita)
                .ThenBy(c => c.Turno)
                .ToList();

            await LoadNavigationProperties(citas);
            return citas;
        }

        private async Task LoadNavigationProperties(List<Cita> citas)
        {
            foreach (var cita in citas)
            {
                cita.Cliente = await _usuarioService.GetUsuarioByIdAsync(cita.ClienteId);
                cita.Servicio = await _servicioService.GetServicioByIdAsync(cita.ServicioId);
            }
        }
    }
}