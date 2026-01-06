using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElegantnailsstudioSystemManagement.Services;
using ElegantnailsstudioSystemManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ElegantnailsstudioSystemManagement.Tests.Services
{
    [TestClass]
    public class CupoServiceTests
    {
        private ApplicationDbContext _context;
        private CupoService _service;

        [TestInitialize]
        public void Setup()
        {
            
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new CupoService(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task IsTurnoPasadoAsync_TurnoDeAyer_ShouldReturnTrue()
        {
            
            DateTime fechaAyer = DateTime.Today.AddDays(-1);
            string turno = "mañana";

            var result = await _service.IsTurnoPasadoAsync(fechaAyer, turno);

           
            Assert.IsTrue(result, "Turno de ayer debería estar pasado");
        }

        [TestMethod]
        public async Task IsTurnoPasadoAsync_TurnoDeManana_ShouldReturnFalse()
        {
            
            DateTime fechaManana = DateTime.Today.AddDays(1);
            string turno = "tarde";

            
            var result = await _service.IsTurnoPasadoAsync(fechaManana, turno);

           
            Assert.IsFalse(result, "Turno de mañana no debería estar pasado");
        }

        [TestMethod]
        public async Task HabilitarTurnoAsync_NuevoTurno_ShouldCreateCupo()
        {
            
            DateTime fecha = DateTime.Today.AddDays(2);
            string turno = "mañana";
            int cuposMaximos = 8;

           
            var result = await _service.HabilitarTurnoAsync(fecha, turno, cuposMaximos);

            
            Assert.IsTrue(result, "Debería habilitar exitosamente");

            var cupoCreado = await _context.Cupos.FirstOrDefaultAsync(c =>
                c.Fecha == fecha.Date && c.Turno == turno);

            Assert.IsNotNull(cupoCreado, "Debería existir en la base de datos");
            Assert.AreEqual(8, cupoCreado.CupoMaximo);
            Assert.AreEqual(0, cupoCreado.CupoReservado);
            Assert.IsTrue(cupoCreado.Habilitado);
        }

        [TestMethod]
        public async Task HabilitarTurnoAsync_TurnoExistente_ShouldUpdateCupo()
        {
            
            DateTime fecha = DateTime.Today.AddDays(3);
            string turno = "tarde";

            
            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 2,
                Habilitado = false
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.HabilitarTurnoAsync(fecha, turno, 10);

            
            Assert.IsTrue(result, "Debería actualizar exitosamente");

            var cupoActualizado = await _context.Cupos.FirstAsync();
            Assert.AreEqual(10, cupoActualizado.CupoMaximo, "Debería actualizar cupos máximos");
            Assert.AreEqual(2, cupoActualizado.CupoReservado, "Debería mantener reservas existentes");
            Assert.IsTrue(cupoActualizado.Habilitado, "Debería estar habilitado");
        }

        [TestMethod]
        public async Task CheckDisponibilidadAsync_CupoDisponible_ShouldReturnTrue()
        {
           
            DateTime fecha = DateTime.Today.AddDays(4);
            string turno = "mañana";

            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 2,
                Habilitado = true
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.CheckDisponibilidadAsync(fecha, turno, 60);

            
            Assert.IsTrue(result, "Debería haber disponibilidad (3 cupos libres)");
        }

        [TestMethod]
        public async Task CheckDisponibilidadAsync_CupoLleno_ShouldReturnFalse()
        {
            
            DateTime fecha = DateTime.Today.AddDays(5);
            string turno = "tarde";

            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 5,
                Habilitado = true
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.CheckDisponibilidadAsync(fecha, turno, 60);

           
            Assert.IsFalse(result, "No debería haber disponibilidad (cupo lleno)");
        }

        [TestMethod]
        public async Task CheckDisponibilidadAsync_TurnoNoHabilitado_ShouldReturnFalse()
        {
            
            DateTime fecha = DateTime.Today.AddDays(6);
            string turno = "mañana";

            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 2,
                Habilitado = false 
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.CheckDisponibilidadAsync(fecha, turno, 60);

            
            Assert.IsFalse(result, "No debería haber disponibilidad (turno no habilitado)");
        }

        [TestMethod]
        public async Task CheckDisponibilidadAsync_TurnoInexistente_ShouldReturnFalse()
        {
           
            DateTime fecha = DateTime.Today.AddDays(7);
            string turno = "mañana";
           
            var result = await _service.CheckDisponibilidadAsync(fecha, turno, 60);

            
            Assert.IsFalse(result, "No debería haber disponibilidad (turno no existe)");
        }

        [TestMethod]
        public async Task ReservarCupoAsync_CupoDisponible_ShouldReserveAndReturnTrue()
        {
           
            DateTime fecha = DateTime.Today.AddDays(8);
            string turno = "mañana";

            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 2,
                Habilitado = true
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.ReservarCupoAsync(fecha, turno);

           
            Assert.IsTrue(result, "Debería reservar exitosamente");

            var cupoActualizado = await _context.Cupos.FirstAsync();
            Assert.AreEqual(3, cupoActualizado.CupoReservado, "Debería tener 3 reservados después de reservar");
        }

        [TestMethod]
        public async Task ReservarCupoAsync_CupoLleno_ShouldReturnFalse()
        {
            
            DateTime fecha = DateTime.Today.AddDays(9);
            string turno = "tarde";

            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 5,
                Habilitado = true
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.ReservarCupoAsync(fecha, turno);

           
            Assert.IsFalse(result, "No debería reservar (cupo lleno)");
            Assert.AreEqual(5, _context.Cupos.First().CupoReservado, "No debería cambiar el número de reservas");
        }

        [TestMethod]
        public async Task LiberarCupoAsync_CupoConReservas_ShouldReleaseAndReturnTrue()
        {
           
            DateTime fecha = DateTime.Today.AddDays(10);
            string turno = "mañana";

            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 3,
                Habilitado = true
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.LiberarCupoAsync(fecha, turno);

            
            Assert.IsTrue(result, "Debería liberar exitosamente");

            var cupoActualizado = await _context.Cupos.FirstAsync();
            Assert.AreEqual(2, cupoActualizado.CupoReservado, "Debería tener 2 reservados después de liberar");
        }

        [TestMethod]
        public async Task LiberarCupoAsync_CupoSinReservas_ShouldReturnFalse()
        {
            
            DateTime fecha = DateTime.Today.AddDays(11);
            string turno = "tarde";

            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 0, 
                Habilitado = true
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.LiberarCupoAsync(fecha, turno);

           
            Assert.IsFalse(result, "No debería liberar (sin reservas)");
            Assert.AreEqual(0, _context.Cupos.First().CupoReservado, "Debería mantener 0 reservas");
        }

        [TestMethod]
        public async Task GetCupoByFechaTurnoAsync_ExistingCupo_ShouldReturnCupo()
        {
            
            DateTime fecha = DateTime.Today.AddDays(12);
            string turno = "mañana";

            var expectedCupo = new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 8,
                CupoReservado = 4,
                Habilitado = true
            };
            _context.Cupos.Add(expectedCupo);
            await _context.SaveChangesAsync();

            
            var result = await _service.GetCupoByFechaTurnoAsync(fecha, turno);

            
            Assert.IsNotNull(result, "Debería encontrar el cupo");
            Assert.AreEqual(8, result.CupoMaximo);
            Assert.AreEqual(4, result.CupoReservado);
            Assert.IsTrue(result.Habilitado);
        }

        [TestMethod]
        public async Task GetCupoByFechaTurnoAsync_NonExistingCupo_ShouldReturnNull()
        {
           
            DateTime fecha = DateTime.Today.AddDays(13);
            string turno = "tarde";
            

            var result = await _service.GetCupoByFechaTurnoAsync(fecha, turno);

            
            Assert.IsNull(result, "Debería retornar null para cupo inexistente");
        }

        [TestMethod]
        public async Task GetCuposByFechaAsync_MultipleCupos_ShouldReturnList()
        {
           
            DateTime fecha = DateTime.Today.AddDays(14);

            _context.Cupos.AddRange(
                new Cupo { Fecha = fecha, Turno = "mañana", Habilitado = true },
                new Cupo { Fecha = fecha, Turno = "tarde", Habilitado = true },
                new Cupo { Fecha = fecha.AddDays(1), Turno = "mañana", Habilitado = true } // Día diferente
            );
            await _context.SaveChangesAsync();

           
            var result = await _service.GetCuposByFechaAsync(fecha);

           
            Assert.AreEqual(2, result.Count, "Debería retornar 2 cupos para la fecha especificada");
            Assert.AreEqual("mañana", result[0].Turno, "Debería estar ordenado por turno");
            Assert.AreEqual("tarde", result[1].Turno);
        }

        [TestMethod]
        public async Task HayCupoDisponibleAsync_CupoDisponible_ShouldReturnTrue()
        {
           
            DateTime fecha = DateTime.Today.AddDays(15);
            string turno = "mañana";

            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 2,
                Habilitado = true
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.HayCupoDisponibleAsync(fecha, turno);

           
            Assert.IsTrue(result, "Debería haber cupo disponible");
        }

        [TestMethod]
        public async Task HayCupoDisponibleAsync_CupoLleno_ShouldReturnFalse()
        {
           
            DateTime fecha = DateTime.Today.AddDays(16);
            string turno = "tarde";

            _context.Cupos.Add(new Cupo
            {
                Fecha = fecha,
                Turno = turno,
                CupoMaximo = 5,
                CupoReservado = 5,
                Habilitado = true
            });
            await _context.SaveChangesAsync();

            
            var result = await _service.HayCupoDisponibleAsync(fecha, turno);

            
            Assert.IsFalse(result, "No debería haber cupo disponible (lleno)");
        }

        [TestMethod]
        public async Task DeshabilitarTurnosPasadosAsync_ShouldDisablePastTurns()
        {
            
            var hoy = DateTime.Today;

            _context.Cupos.AddRange(
                new Cupo { Fecha = hoy.AddDays(-1), Turno = "mañana", Habilitado = true }, 
                new Cupo { Fecha = hoy, Turno = "mañana", Habilitado = true }, 
                new Cupo { Fecha = hoy, Turno = "tarde", Habilitado = true }, 
                new Cupo { Fecha = hoy.AddDays(1), Turno = "mañana", Habilitado = true }  
            );
            await _context.SaveChangesAsync();

           
            var result = await _service.DeshabilitarTurnosPasadosAsync();

           
            Assert.IsTrue(result, "Debería haber deshabilitado turnos");

            var cupos = await _context.Cupos.ToListAsync();
            var ayer = cupos.First(c => c.Fecha == hoy.AddDays(-1));
            var hoyManana = cupos.First(c => c.Fecha == hoy && c.Turno == "mañana");
            var hoyTarde = cupos.First(c => c.Fecha == hoy && c.Turno == "tarde");
            var manana = cupos.First(c => c.Fecha == hoy.AddDays(1));

            Assert.IsFalse(ayer.Habilitado, "Turno de ayer debería estar deshabilitado");
           
        }
    }
}