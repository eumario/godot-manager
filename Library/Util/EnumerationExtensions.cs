using System.Collections.Generic;
using System.Linq;

namespace GodotManager.Library.Util;

public static class EnumerationExtensions
{
    public static IEnumerable<(T ItemEntry, int index)> WithIndex<T>(this IEnumerable<T> source) =>
        source.Select((item, index) => (item, index));
}