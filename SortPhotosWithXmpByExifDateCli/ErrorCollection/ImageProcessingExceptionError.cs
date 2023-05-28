using MetadataExtractor;

namespace SortPhotosWithXmpByExifDateCli.Statistics;
public sealed class ImageProcessingExceptionError : ExceptionError
{
    public ImageProcessingExceptionError(string file, ImageProcessingException exception)
    : base(file, exception, new List<string>() { nameof(ImageProcessingException) + ": " + exception.Message })
    {
    }
}