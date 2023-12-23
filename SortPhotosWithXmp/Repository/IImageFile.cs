using SystemInterface;
using SystemInterface.IO;

namespace SortPhotosWithXmp.Repository;

public interface IImageFile
{
    IFile File { get; }
    string OriginalFilename { get; }
    IDateTime LastWriteTimeUtc { get; }
    public bool IsModified => LastWriteTimeUtc != File.GetLastWriteTime(CurrentFilename);
    public string? NewFilename { get; set; }
    public string CurrentFilename { get; }
}
