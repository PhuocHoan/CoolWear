using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Services;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T>
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    void UpdateById(int id, T entity);
    void Update(T entity);
    void DeleteById(int id);
    void Delete(T entity);
    int Count();
}
