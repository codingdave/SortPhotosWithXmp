namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
public sealed class ImageProcessingExceptionError : ExceptionError
{
    public ImageProcessingExceptionError(string file, MetadataExtractor.ImageProcessingException exception)
    : base(file, exception, new List<string>() { nameof(MetadataExtractor.ImageProcessingException) + ": " + exception.Message })
    {
    }
}