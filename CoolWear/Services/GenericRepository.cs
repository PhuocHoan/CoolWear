using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoolWear.Services;

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAsync(ISpecification<T> spec) => await ApplySpecification(spec).ToListAsync();
    public async Task<int> CountAsync(ISpecification<T> spec) =>
        // Apply only the criteria for counting
        await ApplySpecification(spec).CountAsync();
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);

    public async Task AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity); // Call SaveChangesAsync() to save
    }

    public Task UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Attach(entity); // Attach if not tracked
        _dbSet.Entry(entity).State = EntityState.Modified;

        return Task.CompletedTask; // Call SaveChangesAsync() to save
    }

    public Task DeleteAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // If entity is already tracked, Remove works directly.
        // If not tracked, Attach first then Remove.
        if (_dbSet.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);

        return Task.CompletedTask; // Call SaveChangesAsync() to save
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        var query = _dbSet.AsQueryable();

        // Apply filtering criteria
        if (spec.Criteria.Any())
        {
            foreach (var criteria in spec.Criteria)
            {
                query = query.Where(criteria);
            }
        }

        // Include related entities using strings
        if (spec.IncludeStrings.Any()) // Use IncludeStrings
        {
            query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));
        }

        if (spec.Take > 0)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        return query;
    }
}
