using MetadataExtractor;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public sealed class ImageProcessingExceptionError : ErrorBase
{
    public ImageProcessingExceptionError(FileInfo fileInfo, ImageProcessingException exception)
    : base(fileInfo, new List<string>() { nameof(ImageProcessingException) + ": " + exception.Message })
    {
        Exception = exception;
    }

    public ImageProcessingException Exception { get; }
}