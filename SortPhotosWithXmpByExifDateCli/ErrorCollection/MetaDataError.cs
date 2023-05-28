namespace SortPhotosWithXmpByExifDateCli.Statistics;

public sealed class MetaDataError : ErrorBase
{
    public MetaDataError(string file, IEnumerable<string> messages)
    : base(file, messages)
    {
    }

    public override string Name => nameof(MetaDataError);
}