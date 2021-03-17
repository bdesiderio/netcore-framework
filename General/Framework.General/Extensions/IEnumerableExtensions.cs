using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.General.Extensions
{
    public static class IEnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || source.Count() == 0;
        }
    }
}
