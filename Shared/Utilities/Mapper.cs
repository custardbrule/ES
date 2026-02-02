using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Mapper
    {
        public static IEnumerable<T> MapMemory<TSource, T>(this IEnumerable<TSource> sources, Func<TSource, T> mapperFunc) => sources.Select(mapperFunc);
        public static IQueryable<T> Map<TSource, T>(this IQueryable<TSource> sources, Expression<Func<TSource, T>> mapperExpr) => sources.Select(mapperExpr);
    }
}
