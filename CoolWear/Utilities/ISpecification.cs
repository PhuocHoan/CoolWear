using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CoolWear.Utilities;

/// <summary>
/// Interface for implementing the Specification pattern with dynamic querying capabilities.
/// </summary>
/// <typeparam name="T">The type of entity to which this specification applies.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Collection of filter expressions to be applied to the query.
    /// </summary>
    IEnumerable<Expression<Func<T, bool>>> Criteria { get; }

    /// <summary>
    /// Collection of include expressions for eager loading related entities.
    /// </summary>
    IEnumerable<string> IncludeStrings { get; } // String based Includes

    // --- Pagination ---
    /// <summary>
    /// Gets the number of items to take (page size).
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Gets the number of items to skip.
    /// </summary>
    int Skip { get; }
}
