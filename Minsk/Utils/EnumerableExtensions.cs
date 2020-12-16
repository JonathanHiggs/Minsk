using System;
using System.Collections.Generic;
using System.Linq;

namespace Minsk.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T1, T2)> CrossJoin<T1, T2>(
            this IEnumerable<T1> source,
            IEnumerable<T2> sequence)
            => source.SelectMany(t1 => sequence.Select(t2 => (t1, t2)));

        public static IEnumerable<TResult> CrossJoin<T1, T2, TResult>(
            this IEnumerable<T1> source,
            IEnumerable<T2> sequence,
            Func<T1, T2, TResult> selector)
            => source.SelectMany(t1 => sequence.Select(t2 => selector(t1, t2)));

        public static IEnumerable<TResult> CartesianJoin<T1, TResult>(
            this IEnumerable<T1> source,
            Func<T1, T1, TResult> selector)
            => source.SelectMany(t1 => source.Select(t2 => selector(t1, t2)));
    }
}
