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
        /// <summary>
        /// use for mapping in LINQ queries - uses direct expression invocation for SQL translation
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="mapperExpr"></param>
        /// <returns></returns>
        public static T Map<TSource, T>(this TSource source, Expression<Func<TSource, T>> mapperExpr) => mapperExpr.Compile()(source);

        /// <summary>
        /// use for in-memory mapping - more efficient for non-queryable operations
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="mapperFunc">function must be precompile if use for linq</param>
        /// <returns></returns>
        public static T MapMemory<TSource, T>(this TSource source, Func<TSource, T> mapperFunc) => mapperFunc(source);
    }
}
