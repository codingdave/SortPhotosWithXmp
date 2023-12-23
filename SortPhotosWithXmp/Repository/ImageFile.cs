using SystemInterface;
using SystemInterface.IO;

namespace SortPhotosWithXmp.Repository;

public record struct ImageFile(string OriginalFilename, IDateTime LastWriteTimeUtc, IFile File) : IImageFile
{
#warning Update time when setting NewFilename?
    public ImageFile(string originalFilename, IFile file) : this(originalFilename, file.GetLastWriteTimeUtc(originalFilename), file) { }
    public string? NewFilename { get; set; } = null;
    public string CurrentFilename => NewFilename ?? OriginalFilename;
}
