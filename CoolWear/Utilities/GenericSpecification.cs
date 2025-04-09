using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CoolWear.Utilities;
public class GenericSpecification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, bool>>> _criteria = [];
    private readonly List<string> _includeStrings = [];

    public GenericSpecification() { }

    public IEnumerable<Expression<Func<T, bool>>> Criteria => _criteria;
    public IEnumerable<string> IncludeStrings => _includeStrings;

    public int Take { get; private set; }
    public int Skip { get; private set; }

    public void AddCriteria(Expression<Func<T, bool>> criteria) => _criteria.Add(criteria);
    public void AddInclude(string includeString) => _includeStrings.Add(includeString);

    /// <summary>
    /// Enables pagination for this specification.
    /// </summary>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to take (page size).</param>
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }
}

// Helper class để tạo biểu thức (And, Or)
public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }
}