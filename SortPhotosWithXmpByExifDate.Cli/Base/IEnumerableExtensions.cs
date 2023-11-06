namespace SortPhotosWithXmpByExifDate.Cli.Extensions;

public static class IEnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach(var item in collection)
        {
            action(item);
        }
    }
}