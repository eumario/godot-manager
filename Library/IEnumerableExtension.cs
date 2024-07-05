using System.Collections.Generic;
using System.Linq;

namespace GodotManager.Library;

public static class IEnumerableExtension
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
}