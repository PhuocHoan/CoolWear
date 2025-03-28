using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Utilities;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Filter { get; }
    Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; }
    int? PageNumber { get; }
    int? PageSize { get; }
}