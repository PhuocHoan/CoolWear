using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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

    public async Task AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity); // Call SaveChangesAsync() to save
    }

    public Task UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Attach(entity); // Attach if not tracked
        _context.Entry(entity).State = EntityState.Modified;

        return Task.CompletedTask; // Call SaveChangesAsync() to save
    }

    public Task DeleteAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // If entity is already tracked, Remove works directly.
        // If not tracked, Attach first then Remove.
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);

        return Task.CompletedTask; // Call SaveChangesAsync() to save
    }

    // --- UpdateAsync using ExecuteUpdateAsync (Keep if needed for bulk updates) ---
    // Be cautious as this bypasses change tracking and concurrency checks
    public async Task UpdateAsync(
        ISpecification<T> spec,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression)
    {
        // This method remains for bulk updates if explicitly desired
        var query = ApplySpecification(spec);
        await query.ExecuteUpdateAsync(updateExpression);
    }

    // Modified DeleteAsync using Specification: Fetch then Remove
    public async Task DeleteAsync(ISpecification<T> spec)
    {
        // Fetch entities matching the specification
        var entitiesToDelete = await ApplySpecification(spec).ToListAsync();

        if (entitiesToDelete.Any())
        {
            _dbSet.RemoveRange(entitiesToDelete);
            // Note: Actual deletion happens on SaveChangesAsync()
        }
    }

    // --- ApplySpecification remains the same ---
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
            query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include)); // Use string overload
        }

        // Add AsNoTracking() if needed (consider adding IsReadOnly to ISpecification)
        // if (spec.IsReadOnly) query = query.AsNoTracking();

        return query;
    }
}
