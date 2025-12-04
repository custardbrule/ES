using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infras.Diary.Services.Mappers
{
    public static class Mapper
    {
        /// <summary>
        /// use for mapping
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="mapperExpr"></param>
        /// <returns></returns>
        public static T Map<TSource, T>(this TSource source, Expression<Func<TSource, T>> mapperExpr) => mapperExpr.Compile()(source);

        /// <summary>
        /// use for mapping
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="mapperFunc">function must be precompile if use for linq</param>
        /// <returns></returns>
        public static T Map<TSource, T>(this TSource source, Func<TSource, T> mapperFunc) => mapperFunc(source);
    }
}
