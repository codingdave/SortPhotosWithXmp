namespace SortPhotosWithXmpByExifDateCli.Statistics;

public sealed class MetaDataError : ErrorBase
{
    public MetaDataError(FileInfo fileInfo, IEnumerable<string> messages)
    : base(fileInfo, messages)
    {
    }

    public override string Name => nameof(MetaDataError);
}