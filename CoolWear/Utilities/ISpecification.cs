using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CoolWear.Utilities;

/// <summary>
/// Interface for implementing the Specification pattern with dynamic querying capabilities.
/// </summary>
/// <typeparam name="T">The type of entity to which this specification applies.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets a collection of filter expressions to be applied to the query.
    /// </summary>
    IEnumerable<Expression<Func<T, bool>>> Criteria { get; }

    /// <summary>
    /// Gets a collection of include expressions for eager loading related entities.
    /// </summary>
    IEnumerable<string> IncludeStrings { get; } // String based Includes
}
