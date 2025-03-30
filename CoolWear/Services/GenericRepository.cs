using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoolWear.Services;

public class GenericRepository<T>(DbContext context) : IRepository<T> where T : class
{
    private readonly DbContext _context = context;
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        // For entities with Id property - handles most common cases
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _dbSet.AddAsync(entity);
        // Note: Changes aren't saved to the database until UnitOfWork.SaveChangesAsync() is called
    }

    public async Task UpdateAsync(
        ISpecification<T> spec,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression)
    {
        var query = ApplySpecification(spec);
        await query.ExecuteUpdateAsync(updateExpression);
    }

    public async Task DeleteAsync(ISpecification<T> spec)
    {
        var query = ApplySpecification(spec);
        await query.ExecuteDeleteAsync();
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

        // Include related entities
        if (spec.Includes.Any())
        {
            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        }

        return query;
    }
}
