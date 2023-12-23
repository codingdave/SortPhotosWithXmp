using SystemInterface;
using SystemInterface.IO;

namespace SortPhotosWithXmp.Repository;

public record struct ImageFileHash(string OriginalFilename, ulong Hash, IDateTime LastWriteTimeUtc, IFile File) : IImageFile, IPerceptualHash
{
    public string? NewFilename { get; set; } = null;
    public string CurrentFilename => NewFilename ?? OriginalFilename;
}


