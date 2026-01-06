using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElegantnailsstudioSystemManagement.Services;
using ElegantnailsstudioSystemManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElegantnailsstudioSystemManagement.Tests.Services
{
    [TestClass]
    public class CitaServiceTests
    {
        private ApplicationDbContext _context;
        private CupoService _cupoService;
        private CitaService _citaService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _cupoService = new CupoService(_context);
            _citaService = new CitaService(_context, _cupoService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        // Método para crear cliente real para pruebas
        private Usuario CrearClienteReal(int id, string nombre, string telefono = "555-1234")
        {
            return new Usuario
            {
                Id = id,
                Nombre = nombre,
                Email = $"{nombre.ToLower().Replace(" ", "")}@email.com",
                Password = "Password123", // Requerido por validaciones
                telefono = telefono,
                rolid = 2 // Cliente
            };
        }

        // Método para crear servicio real para pruebas
        private Servicio CrearServicioReal(int id, string nombre, decimal precio, int duracion)
        {
            return new Servicio
            {
                Id = id,
                Nombre = nombre,
                Precio = precio,
                DuracionMinutos = duracion
            };
        }

        [TestMethod]
        public async Task CreateCitaAsync_CitaValidaParaManana_TurnoManana_DeberiaCrearExitosamente()
        {
            // Arrange - Escenario REAL: Cliente quiere cita para mañana en turno mañana
            DateTime fechaManana = DateTime.Today.AddDays(1);
            string turno = "mañana";

            // 1. Habilitar turno (como haría el administrador)
            await _cupoService.HabilitarTurnoAsync(fechaManana, turno, cuposMaximos: 8);

            // 2. Crear servicio de manicure (servicio real)
            var servicio = CrearServicioReal(1, "Manicure Tradicional", 150.00m, 60);

            // 3. Crear cliente real
            var cliente = CrearClienteReal(1, "María González", "555-123-4567");

            _context.Servicios.Add(servicio);
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            // 4. Crear cita (como haría el cliente)
            var cita = new Cita
            {
                ClienteId = 1,
                ServicioId = 1,
                FechaCita = fechaManana,
                Turno = turno,
                Estado = "pendiente"
            };

            // Act - Crear la cita
            var result = await _citaService.CreateCitaAsync(cita);

            // Assert - Verificar resultados reales
            Assert.IsTrue(result, "La cita debería crearse exitosamente");

            // Verificar en base de datos
            var citaGuardada = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .FirstOrDefaultAsync();

            Assert.IsNotNull(citaGuardada, "La cita debería existir en BD");
            Assert.AreEqual("pendiente", citaGuardada.Estado);
            Assert.AreEqual("María González", citaGuardada.Cliente.Nombre);
            Assert.AreEqual("Manicure Tradicional", citaGuardada.Servicio.Nombre);
            Assert.IsNotNull(citaGuardada.FechaCreacion);

            // Verificar que se reservó cupo
            var cupo = await _cupoService.GetCupoByFechaTurnoAsync(fechaManana, turno);
            Assert.AreEqual(1, cupo.CupoReservado, "Debería haber 1 cupo reservado");
        }

        [TestMethod]
        public async Task CreateCitaAsync_CitaParaFechaPasada_DeberiaFallar()
        {
            // Arrange - Escenario REAL: Cliente intenta agendar cita para ayer (error común)
            var servicio = CrearServicioReal(1, "Pedicure Spa", 250.00m, 90);
            var cliente = CrearClienteReal(1, "Carlos López", "555-987-6543");

            _context.Servicios.Add(servicio);
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            // Cita para ayer (fecha pasada)
            var cita = new Cita
            {
                ClienteId = 1,
                ServicioId = 1,
                FechaCita = DateTime.Today.AddDays(-1),
                Turno = "tarde",
                Estado = "pendiente"
            };

            // Act
            var result = await _citaService.CreateCitaAsync(cita);

            // Assert
            Assert.IsFalse(result, "No debería permitir citas en fechas pasadas");
        }

        [TestMethod]
        public async Task CreateCitaAsync_TurnoNoHabilitado_DeberiaFallar()
        {
            // Arrange - Escenario REAL: Cliente intenta agendar en turno no habilitado
            var servicio = CrearServicioReal(1, "Uñas Acrílicas", 350.00m, 120);
            var cliente = CrearClienteReal(1, "Ana Torres", "555-456-7890");

            _context.Servicios.Add(servicio);
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            // NO habilitar el turno (simula que el admin no abrió ese horario)
            var cita = new Cita
            {
                ClienteId = 1,
                ServicioId = 1,
                FechaCita = DateTime.Today.AddDays(2),
                Turno = "mañana",
                Estado = "pendiente"
            };

            // Act
            var result = await _citaService.CreateCitaAsync(cita);

            // Assert
            Assert.IsFalse(result, "No debería permitir citas en turnos no habilitados");
        }

        [TestMethod]
        public async Task CancelarCitaAsync_ClienteCancelaSuCita_DeberiaPermitir()
        {
            // Arrange - Escenario REAL: Cliente cancela su propia cita
            DateTime fechaCita = DateTime.Today.AddDays(3);
            string turno = "tarde";

            // 1. Habilitar turno
            await _cupoService.HabilitarTurnoAsync(fechaCita, turno, 6);

            // 2. Crear servicio y cliente
            var servicio = CrearServicioReal(1, "Manicure French", 180.00m, 75);
            var cliente = CrearClienteReal(1, "Laura Martínez", "555-111-2233");

            _context.Servicios.Add(servicio);
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            // 3. Crear cita
            var cita = new Cita
            {
                ClienteId = 1,
                ServicioId = 1,
                FechaCita = fechaCita,
                Turno = turno,
                Estado = "pendiente"
            };

            await _citaService.CreateCitaAsync(cita);
            var citaCreada = await _context.Citas.FirstAsync();

            // Act - Cliente cancela su cita
            var result = await _citaService.CancelarCitaAsync(citaCreada.Id, 1);

            // Assert
            Assert.IsTrue(result, "El cliente debería poder cancelar su cita");

            var citaCancelada = await _context.Citas.FindAsync(citaCreada.Id);
            Assert.AreEqual("cancelada", citaCancelada.Estado);
            Assert.IsNotNull(citaCancelada.FechaCancelacion);

            // Verificar que se liberó el cupo
            var cupo = await _cupoService.GetCupoByFechaTurnoAsync(fechaCita, turno);
            Assert.AreEqual(0, cupo.CupoReservado, "El cupo debería liberarse");
        }

        [TestMethod]
        public async Task CancelarCitaAsync_OtroClienteIntentaCancelar_DeberiaFallar()
        {
            // Arrange - Escenario REAL: Un cliente intenta cancelar cita de otro cliente
            DateTime fechaCita = DateTime.Today.AddDays(4);

            await _cupoService.HabilitarTurnoAsync(fechaCita, "mañana", 5);

            var servicio = CrearServicioReal(1, "Pedicure Regular", 200.00m, 60);
            var cliente1 = CrearClienteReal(1, "Cliente Propietario", "555-999-8888");
            var cliente2 = CrearClienteReal(2, "Cliente Intruso", "555-777-6666"); // Otro cliente

            _context.Servicios.Add(servicio);
            _context.Usuarios.AddRange(cliente1, cliente2);
            await _context.SaveChangesAsync();

            var cita = new Cita
            {
                ClienteId = 1, // Cita del cliente 1
                ServicioId = 1,
                FechaCita = fechaCita,
                Turno = "mañana",
                Estado = "pendiente"
            };

            await _citaService.CreateCitaAsync(cita);
            var citaCreada = await _context.Citas.FirstAsync();

            // Act - Cliente 2 intenta cancelar cita del cliente 1
            var result = await _citaService.CancelarCitaAsync(citaCreada.Id, 2);

            // Assert
            Assert.IsFalse(result, "No debería permitir cancelar cita ajena");

            var citaNoCancelada = await _context.Citas.FindAsync(citaCreada.Id);
            Assert.AreEqual("pendiente", citaNoCancelada.Estado, "La cita debería seguir pendiente");
        }

        [TestMethod]
        public async Task CancelarCitaAdminAsync_AdminCancelaCitaCliente_DeberiaPermitir()
        {
            // Arrange - Escenario REAL: Administrador cancela cita de cliente
            DateTime fechaCita = DateTime.Today.AddDays(5);

            await _cupoService.HabilitarTurnoAsync(fechaCita, "tarde", 4);

            var servicio = CrearServicioReal(1, "Uñas Esculpidas", 400.00m, 120);
            var cliente = CrearClienteReal(1, "Sofía Ramírez", "555-333-4444");

            _context.Servicios.Add(servicio);
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            var cita = new Cita
            {
                ClienteId = 1,
                ServicioId = 1,
                FechaCita = fechaCita,
                Turno = "tarde",
                Estado = "pendiente"
            };

            await _citaService.CreateCitaAsync(cita);
            var citaCreada = await _context.Citas.FirstAsync();

            // Act - Admin cancela la cita
            var result = await _citaService.CancelarCitaAdminAsync(citaCreada.Id);

            // Assert
            Assert.IsTrue(result, "Admin debería poder cancelar cualquier cita");

            var citaCancelada = await _context.Citas.FindAsync(citaCreada.Id);
            Assert.AreEqual("cancelada", citaCancelada.Estado);
        }

        [TestMethod]
        public async Task CompletarCitaAsync_MarcarCitaComoCompletada_DeberiaFuncionar()
        {
            // Arrange - Escenario REAL: Se completa una cita después del servicio
            var servicio = CrearServicioReal(1, "Manicure Básico", 150.00m, 60);
            var cliente = CrearClienteReal(1, "Cliente Prueba", "555-000-1111");

            _context.Servicios.Add(servicio);
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            // Crear cita directamente en BD (simula cita existente)
            var cita = new Cita
            {
                Id = 100,
                ClienteId = 1,
                ServicioId = 1,
                FechaCita = DateTime.Today.AddHours(2),
                Turno = "mañana",
                Estado = "pendiente",
                FechaCreacion = DateTime.Now.AddHours(-1)
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            // Act - Marcar como completada
            var result = await _citaService.CompletarCitaAsync(100);

            // Assert
            Assert.IsTrue(result, "Debería poder completar la cita");

            var citaCompletada = await _context.Citas.FindAsync(100);
            Assert.AreEqual("completada", citaCompletada.Estado);
        }

       
        

        [TestMethod]
        public async Task GetCitasByEstadoAsync_DeberiaFiltrarPorEstado()
        {
            // Arrange - Escenario REAL: Filtrar citas por estado (para reportes)
            var cliente = CrearClienteReal(1, "Cliente Filtro", "555-888-9999");
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            var citas = new List<Cita>
            {
                new Cita { Id = 1, ClienteId = 1, Estado = "pendiente", FechaCita = DateTime.Today.AddDays(1), Cliente = cliente },
                new Cita { Id = 2, ClienteId = 1, Estado = "completada", FechaCita = DateTime.Today.AddDays(2), Cliente = cliente },
                new Cita { Id = 3, ClienteId = 1, Estado = "pendiente", FechaCita = DateTime.Today.AddDays(3), Cliente = cliente }
            };

            _context.Citas.AddRange(citas);
            await _context.SaveChangesAsync();

            // Act - Filtrar por "pendiente"
            var result = await _citaService.GetCitasByEstadoAsync("pendiente");

            // Assert
            Assert.AreEqual(2, result.Count, "Debería retornar 2 citas pendientes");
            Assert.IsTrue(result.All(c => c.Estado == "pendiente"), "Todas deberían estar pendientes");
        }

        [TestMethod]
        public async Task UpdateCitaAsync_ReagendarCita_DeberiaActualizarCorrectamente()
        {
            // Arrange - Escenario REAL: Cliente reagenda su cita
            DateTime fechaOriginal = DateTime.Today.AddDays(6);
            DateTime fechaNueva = DateTime.Today.AddDays(7);

            // Habilitar ambos turnos
            await _cupoService.HabilitarTurnoAsync(fechaOriginal, "mañana", 5);
            await _cupoService.HabilitarTurnoAsync(fechaNueva, "tarde", 5);

            var servicio = CrearServicioReal(1, "Servicio Prueba", 100.00m, 60);
            var cliente = CrearClienteReal(1, "Cliente Reagenda", "555-555-5555");

            _context.Servicios.Add(servicio);
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            // Crear cita original
            var citaOriginal = new Cita
            {
                ClienteId = 1,
                ServicioId = 1,
                FechaCita = fechaOriginal,
                Turno = "mañana",
                Estado = "pendiente"
            };

            await _citaService.CreateCitaAsync(citaOriginal);
            var citaCreada = await _context.Citas.FirstAsync();

            // Preparar cita actualizada (reagendada)
            var citaActualizada = new Cita
            {
                Id = citaCreada.Id,
                ClienteId = 1,
                ServicioId = 1,
                FechaCita = fechaNueva, // Nueva fecha
                Turno = "tarde",        // Nuevo turno
                Estado = "pendiente"
            };

            // Act - Actualizar cita
            var result = await _citaService.UpdateCitaAsync(citaActualizada);

            // Assert
            Assert.IsTrue(result, "Debería actualizar la cita exitosamente");

            var citaModificada = await _context.Citas.FindAsync(citaCreada.Id);
            Assert.AreEqual(fechaNueva.Date, citaModificada.FechaCita.Date);
            Assert.AreEqual("tarde", citaModificada.Turno);
        }

        [TestMethod]
        public async Task DeleteCitaAsync_EliminarCita_DeberiaEliminarYLiberarCupo()
        {
            // Arrange - Escenario REAL: Eliminar cita (admin o sistema)
            DateTime fechaCita = DateTime.Today.AddDays(8);

            await _cupoService.HabilitarTurnoAsync(fechaCita, "mañana", 5);

            var servicio = CrearServicioReal(1, "Servicio Eliminar", 120.00m, 45);
            var cliente = CrearClienteReal(1, "Cliente Eliminar", "555-666-7777");

            _context.Servicios.Add(servicio);
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            // Crear cita
            var cita = new Cita
            {
                ClienteId = 1,
                ServicioId = 1,
                FechaCita = fechaCita,
                Turno = "mañana",
                Estado = "pendiente"
            };

            await _citaService.CreateCitaAsync(cita);
            var citaCreada = await _context.Citas.FirstAsync();

            // Verificar cupo reservado
            var cupoAntes = await _cupoService.GetCupoByFechaTurnoAsync(fechaCita, "mañana");
            Assert.AreEqual(1, cupoAntes.CupoReservado, "Debería tener 1 cupo reservado");

            // Act - Eliminar cita
            var result = await _citaService.DeleteCitaAsync(citaCreada.Id);

            // Assert
            Assert.IsTrue(result, "Debería eliminar la cita exitosamente");
            Assert.IsNull(await _context.Citas.FindAsync(citaCreada.Id), "La cita no debería existir en BD");

            // Verificar cupo liberado
            var cupoDespues = await _cupoService.GetCupoByFechaTurnoAsync(fechaCita, "mañana");
            Assert.AreEqual(0, cupoDespues.CupoReservado, "El cupo debería liberarse");
        }


        [TestMethod]
        public async Task GetCitasByFechaAsync_DeberiaFiltrarPorFechaEspecifica()
        {
            // Arrange - Escenario REAL: Ver citas de un día específico
            var cliente = CrearClienteReal(1, "Cliente Fecha", "555-999-0000");
            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            DateTime fechaEspecifica = DateTime.Today.AddDays(1);

            var citas = new List<Cita>
            {
                new Cita { Id = 1, ClienteId = 1, FechaCita = fechaEspecifica, Estado = "pendiente", Cliente = cliente },
                new Cita { Id = 2, ClienteId = 1, FechaCita = fechaEspecifica, Estado = "pendiente", Cliente = cliente },
                new Cita { Id = 3, ClienteId = 1, FechaCita = DateTime.Today.AddDays(2), Estado = "pendiente", Cliente = cliente }
            };

            _context.Citas.AddRange(citas);
            await _context.SaveChangesAsync();

            // Act - Filtrar por fecha específica
            var result = await _citaService.GetCitasByFechaAsync(fechaEspecifica);

            // Assert
            Assert.AreEqual(2, result.Count, "Debería retornar 2 citas para la fecha especificada");
            Assert.IsTrue(result.All(c => c.FechaCita.Date == fechaEspecifica.Date),
                "Todas deberían ser de la fecha especificada");
        }
    }
}