using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoolWear.Services;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task DeleteAsync(T entity);
    Task UpdateAsync(T entity);
}
