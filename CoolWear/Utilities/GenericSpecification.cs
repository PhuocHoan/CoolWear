using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CoolWear.Utilities;
public class GenericSpecification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, bool>>> _criteria = [];
    private readonly List<string> _includeStrings = [];

    public GenericSpecification() { }

    public IEnumerable<Expression<Func<T, bool>>> Criteria => _criteria;
    public IEnumerable<string> IncludeStrings => _includeStrings;

    protected void AddCriteria(Expression<Func<T, bool>> criteria) => _criteria.Add(criteria);
    protected void AddInclude(string includeString) => _includeStrings.Add(includeString);
}