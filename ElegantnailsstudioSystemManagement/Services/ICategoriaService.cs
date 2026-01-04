using ElegantnailsstudioSystemManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ElegantnailsstudioSystemManagement.Services
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> GetCategoriasAsync();
        Task<Categoria?> GetCategoriaByIdAsync(int id);
        Task<Categoria?> GetCategoriaByNombreAsync(string nombre);
        Task<bool> CreateCategoriaAsync(Categoria categoria);
        Task<bool> UpdateCategoriaAsync(Categoria categoria);
        Task<bool> DeleteCategoriaAsync(int id);
    }

    public class CategoriaService : ICategoriaService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CategoriaService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Categorias
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetCategoriasAsync: {ex.Message}");
                return new List<Categoria>();
            }
        }

        public async Task<Categoria?> GetCategoriaByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Categorias.FindAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetCategoriaByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<Categoria?> GetCategoriaByNombreAsync(string nombre)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Categorias
                    .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetCategoriaByNombreAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateCategoriaAsync(Categoria categoria)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                bool existe = await context.Categorias
                    .AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower());

                if (existe)
                    return false;

                context.Categorias.Add(categoria);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR CreateCategoriaAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCategoriaAsync(Categoria categoria)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var existing = await context.Categorias.FindAsync(categoria.Id);
                if (existing == null) return false;

                existing.Nombre = categoria.Nombre;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR UpdateCategoriaAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCategoriaAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var categoria = await context.Categorias.FindAsync(id);
                if (categoria == null) return false;

                context.Categorias.Remove(categoria);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR DeleteCategoriaAsync: {ex.Message}");
                return false;
            }
        }
    }
}
