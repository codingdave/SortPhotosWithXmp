using SystemInterface;
using SystemInterface.IO;

namespace SortPhotosWithXmp.Repository;

public record struct ImageFile(string OriginalFilename, IDateTime LastWriteTimeUtc, IFile FileWrapper) : IImageFile
{
    public ImageFile(string originalFilename, IFile fileWrapper) : this(originalFilename, fileWrapper.GetLastWriteTimeUtc(originalFilename), fileWrapper) { }
    public string? NewFilename { get; set; } = null;
    public string CurrentFilename => NewFilename ?? OriginalFilename;
}
