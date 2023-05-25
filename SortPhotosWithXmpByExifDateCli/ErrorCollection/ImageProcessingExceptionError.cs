using MetadataExtractor;

namespace SortPhotosWithXmpByExifDateCli.Statistics;
public sealed class ImageProcessingExceptionError : ExceptionError
{
    public ImageProcessingExceptionError(FileInfo fileInfo, ImageProcessingException exception)
    : base(fileInfo, exception, new List<string>() { nameof(ImageProcessingException) + ": " + exception.Message })
    {
    }
}