// Minimal sample implementation, referencing an assumed AppDbContext
using CoolWear.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoolWear.Services;

public class GenericRepository<T>(PostgresContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public Task UpdateAsync(T entity)
    {
        _dbSet.Entry(entity).State = EntityState.Modified; // only update changed fields, not force to update all fields
        return Task.CompletedTask;
    }
}
