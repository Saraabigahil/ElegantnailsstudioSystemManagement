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
        private readonly ApplicationDbContext _context;

        public CategoriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            try
            {
                return await _context.Categorias
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
                return await _context.Categorias.FindAsync(id);
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
                return await _context.Categorias
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
                // Evita duplicados
                if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower()))
                    return false;

                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
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
                var existing = await _context.Categorias.FindAsync(categoria.Id);
                if (existing == null) return false;

                existing.Nombre = categoria.Nombre;
                await _context.SaveChangesAsync();
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
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null) return false;

                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
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