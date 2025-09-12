using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infras.Services.Mappers
{
    public static class DailyDiaryMapper
    {
        public static T Map<TSource, T>(this TSource source, Expression<Func<TSource, T>> mapperExpr) => mapperExpr.Compile()(source);
    }
}
