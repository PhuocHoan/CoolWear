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
        // Chỉ áp dụng các tiêu chí để đếm
        await ApplySpecification(spec).CountAsync();
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);

    public async Task AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity); // Gọi SaveChangesAsync() để lưu thay đổi
    }

    public Task UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Attach(entity); // Gắn đối tượng nếu chưa được theo dõi
        _dbSet.Entry(entity).State = EntityState.Modified;

        return Task.CompletedTask; // Gọi SaveChangesAsync() để lưu thay đổi
    }

    public Task DeleteAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Nếu đối tượng đã được theo dõi, phương thức Remove hoạt động trực tiếp.
        // Nếu chưa được theo dõi, cần gắn đối tượng bằng Attach rồi mới gọi Remove.
        if (_dbSet.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);

        return Task.CompletedTask; // Gọi SaveChangesAsync() để lưu thay đổi
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        var query = _dbSet.AsQueryable();

        // Áp dụng các tiêu chí lọc
        if (spec.Criteria.Any())
        {
            foreach (var criteria in spec.Criteria)
            {
                query = query.Where(criteria);
            }
        }

        // Bao gồm các thực thể liên quan bằng cách sử dụng chuỗi
        if (spec.IncludeStrings.Any()) // Sử dụng IncludeStrings
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
