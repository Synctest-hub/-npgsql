using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal
{
    static class IReadOnlyListExtensions
    {
        public static IReadOnlyList<T> Slice<T>([NotNull] this IReadOnlyList<T> list, int start)
            => new IReadOnlyListSlice<T>(list, start);

        sealed class IReadOnlyListSlice<T> : IReadOnlyList<T>
        {
            IReadOnlyList<T> _underlying;
            int _start;

            internal IReadOnlyListSlice(IReadOnlyList<T> underlying, int start)
            {
                _underlying = underlying;
                _start = start;
            }

            public IEnumerator<T> GetEnumerator() => _underlying.Skip(_start).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => _underlying.Count - _start;

            public T this[int index] => _underlying[_start + index];
        }
    }
}
