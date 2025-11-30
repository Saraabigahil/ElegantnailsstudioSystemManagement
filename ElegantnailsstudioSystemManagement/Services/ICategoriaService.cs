using ElegantNailsStudioSystemManagement.Models;

namespace ElegantNailsStudioSystemManagement.Services
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> GetCategoriasAsync();
        Task<Categoria?> GetCategoriaByIdAsync(int id);
        Task<Categoria?> GetCategoriaByNombreAsync(string nombre);
        Task<bool> CreateCategoriaAsync(Categoria categoria);
        Task<bool> UpdateCategoriaAsync(Categoria categoria);
        Task<bool> DeleteCategoriaAsync(int id);
        Task<bool> ToggleCategoriaStatusAsync(int id);
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
                    Nombre = "Uñas",
                    Descripcion = "Servicios de manicura y pedicura",
                    Activo = true
                },
                new Categoria {
                    Id = _nextId++,
                    Nombre = "Pestañas",
                    Descripcion = "Servicios de extensión y diseño de pestañas",
                    Activo = true
                }
            });
        }

        public Task<List<Categoria>> GetCategoriasAsync()
        {
            return Task.FromResult(_categorias.Where(c => c.Activo).ToList());
        }

        public Task<Categoria?> GetCategoriaByIdAsync(int id)
        {
            return Task.FromResult(_categorias.FirstOrDefault(c => c.Id == id && c.Activo));
        }

        public Task<Categoria?> GetCategoriaByNombreAsync(string nombre)
        {
            return Task.FromResult(_categorias.FirstOrDefault(c =>
                c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && c.Activo));
        }

        public Task<bool> CreateCategoriaAsync(Categoria categoria)
        {
            if (_categorias.Any(c => c.Nombre.Equals(categoria.Nombre, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult(false);

            categoria.Id = _nextId++;
            categoria.Activo = true;
            _categorias.Add(categoria);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateCategoriaAsync(Categoria categoria)
        {
            var existing = _categorias.FirstOrDefault(c => c.Id == categoria.Id);
            if (existing != null)
            {
                existing.Nombre = categoria.Nombre;
                existing.Descripcion = categoria.Descripcion;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteCategoriaAsync(int id)
        {
            var categoria = _categorias.FirstOrDefault(c => c.Id == id);
            if (categoria != null)
            {
                categoria.Activo = false;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> ToggleCategoriaStatusAsync(int id)
        {
            var categoria = _categorias.FirstOrDefault(c => c.Id == id);
            if (categoria != null)
            {
                categoria.Activo = !categoria.Activo;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}