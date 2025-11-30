using ElegantnailsstudioSystemManagement.Models;

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
        private readonly List<Categoria> _categorias = new();
        private int _nextId = 1;

        public CategoriaService()
        {
            _categorias.AddRange(new[]
            {
                new Categoria {
                    Id = _nextId++,
                    Nombre = "Uñas"
                },
                new Categoria {
                    Id = _nextId++,
                    Nombre = "Pestañas"
                }
            });
        }

        public Task<List<Categoria>> GetCategoriasAsync()
        {
            return Task.FromResult(_categorias.ToList());
        }

        public Task<Categoria?> GetCategoriaByIdAsync(int id)
        {
            return Task.FromResult(_categorias.FirstOrDefault(c => c.Id == id));
        }

        public Task<Categoria?> GetCategoriaByNombreAsync(string nombre)
        {
            return Task.FromResult(_categorias.FirstOrDefault(c =>
                c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<bool> CreateCategoriaAsync(Categoria categoria)
        {
            if (_categorias.Any(c => c.Nombre.Equals(categoria.Nombre, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult(false);

            categoria.Id = _nextId++;
            _categorias.Add(categoria);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateCategoriaAsync(Categoria categoria)
        {
            var existing = _categorias.FirstOrDefault(c => c.Id == categoria.Id);
            if (existing != null)
            {
                existing.Nombre = categoria.Nombre;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteCategoriaAsync(int id)
        {
            var categoria = _categorias.FirstOrDefault(c => c.Id == id);
            if (categoria != null)
            {
                _categorias.Remove(categoria);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}