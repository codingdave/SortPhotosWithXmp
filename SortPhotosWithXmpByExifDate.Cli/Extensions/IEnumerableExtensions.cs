using System.Diagnostics;

namespace SortPhotosWithXmpByExifDate.Extensions;

public static class IEnumerableExtensions
{
    [DebuggerStepThrough]
    public static void Do<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach(var item in collection)
        {
            action(item);
        }
    }
}