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