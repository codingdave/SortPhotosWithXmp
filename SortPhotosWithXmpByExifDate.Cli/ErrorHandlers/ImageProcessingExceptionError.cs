namespace SortPhotosWithXmpByExifDate.ErrorHandlers;
public sealed class ImageProcessingExceptionError : ExceptionErrorBase
{
    public ImageProcessingExceptionError(string file, MetadataExtractor.ImageProcessingException exception)
    : base(file, exception, new List<string>() { nameof(MetadataExtractor.ImageProcessingException) + ": " + exception.Message })
    {
    }
}