using SystemInterface;
using SystemInterface.IO;

namespace SortPhotosWithXmp.Repository;

public record struct SidecarFileHash(string OriginalFilename, byte[] Hash, IDateTime LastWriteTimeUtc, IFile FileWrapper) : IImageFile, IHash
{
    public string? NewFilename { get; set; } = null;
    public string CurrentFilename => NewFilename ?? OriginalFilename;
}

