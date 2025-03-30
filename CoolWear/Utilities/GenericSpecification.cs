using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CoolWear.Utilities;

/// <summary>
/// Generic implementation of ISpecification that supports dynamic querying.
/// </summary>
/// <typeparam name="T">The type of entity to which this specification applies.</typeparam>
public class GenericSpecification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, bool>>> _criteria = [];
    private readonly List<Expression<Func<T, object>>> _includes = [];

    public GenericSpecification() { }

    public IEnumerable<Expression<Func<T, bool>>> Criteria => _criteria;

    public IEnumerable<Expression<Func<T, object>>> Includes => _includes;

    /// <summary>
    /// Adds a filter criteria to the specification.
    /// </summary>
    protected void AddCriteria(Expression<Func<T, bool>> criteria) => _criteria.Add(criteria);

    /// <summary>
    /// Adds an include expression for eager loading related entities.
    /// </summary>
    protected void AddInclude(Expression<Func<T, object>> includeExpression) => _includes.Add(includeExpression);
}
