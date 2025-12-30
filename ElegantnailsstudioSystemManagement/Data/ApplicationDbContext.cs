using Microsoft.EntityFrameworkCore;
using ElegantnailsstudioSystemManagement.Models;

namespace ElegantnailsstudioSystemManagement
{
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Cupo> Cupos { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Servicio>()
                .HasOne(s => s.Categoria)
                .WithMany(c => c.Servicios)
                .HasForeignKey(s => s.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}








