using CoolWear.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoolWear.Services;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAsync(ISpecification<T> spec);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task UpdateAsync(
        ISpecification<T> spec,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression);
    Task DeleteAsync(T entity);
    Task DeleteAsync(ISpecification<T> spec);
}
