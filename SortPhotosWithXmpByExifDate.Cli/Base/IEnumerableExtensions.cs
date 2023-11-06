using System.Diagnostics;

namespace SortPhotosWithXmpByExifDate.Cli.Extensions;

public static class IEnumerableExtensions
{
    [DebuggerStepThrough]
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach(var item in collection)
        {
            action(item);
        }
    }
}