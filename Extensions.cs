using System;
using System.Collections.Generic;
using System.Linq;

namespace Genjiworlds
{
    public static class Extensions
    {
        private static TType SelectByValue<TType, TValue>(this IEnumerable<TType> items,
            Func<TType, TValue> f,
            Func<TValue, TValue, bool> op)
        {
            if (!items.Any())
                return default(TType);

            TType best = items.First();
            TValue value = f(best);

            foreach (TType item in items)
            {
                TValue value2 = f(item);
                if (op(value, value2))
                {
                    best = item;
                    value = value2;
                }
            }

            return best;
        }

        public static TType MinBy<TType, TValue>(this IEnumerable<TType> items, Func<TType, TValue> f) where TValue : IComparable
        {
            return SelectByValue(items, f, (value, value2) => value2.CompareTo(value) < 0);
        }

        public static TType MaxBy<TType, TValue>(this IEnumerable<TType> items, Func<TType, TValue> f) where TValue : IComparable
        {
            return SelectByValue(items, f, (value, value2) => value2.CompareTo(value) > 0);
        }
    }
}
