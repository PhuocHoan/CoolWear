using CoolWear.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Model;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly List<T> _entities = [];
    private readonly PropertyInfo? _idProperty;

    public Repository()
    {
        _idProperty = FindIdProperty();
    }

    public void Add(T entity)
    {
        _entities.Add(entity);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        _entities.AddRange(entities);
    }

    public int Count()
    {
        return _entities.Count;
    }

    public void Delete(T entity)
    {
        _entities.Remove(entity);
    }

    public void DeleteById(int id)
    {
        if (_idProperty == null)
            return;

        var entity = GetById(id);
        if (entity != null)
            _entities.Remove(entity);
    }

    public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return _entities.AsQueryable().Where(predicate).ToList();
    }

    public IEnumerable<T> GetAll()
    {
        return _entities;
    }

    public T? GetById(int id)
    {
        if (_idProperty == null)
            return default;

        return _entities.FirstOrDefault(e => Convert.ToInt32(_idProperty.GetValue(e)) == id);
    }

    public void Update(T entity)
    {
        if (_idProperty == null)
            return;

        int id = Convert.ToInt32(_idProperty.GetValue(entity));
        UpdateById(id, entity);
    }

    public void UpdateById(int id, T entity)
    {
        if (_idProperty == null)
            return;

        var index = _entities.FindIndex(e => Convert.ToInt32(_idProperty.GetValue(e)) == id);
        if (index >= 0)
            _entities[index] = entity;
    }

    private PropertyInfo? FindIdProperty()
    {
        var typeProperties = typeof(T).GetProperties();

        // Try finding a property that ending with Id or ID
        var property = typeProperties.FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

        return property;
    }
}
