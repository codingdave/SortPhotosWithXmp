using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class FilesAndDirectoriesStatistics : IStatistics, IFoundStatistics
{
    private readonly FilesFoundStatistics _imagesStatistics;
    private readonly DirectoriesDeletedStatistics _directoriesStatistics;

    public FilesAndDirectoriesStatistics(
        FilesFoundStatistics filesStatistics,
        DirectoriesDeletedStatistics directoriesStatistics)
    {
        (_imagesStatistics, _directoriesStatistics) = (filesStatistics, directoriesStatistics);
        FileErrors = new MergedFileError(errorCollection1: _imagesStatistics.FileErrors, errorCollection2: _directoriesStatistics.FileErrors);
    }

    public IReadOnlyErrorCollection FileErrors { get; }
    public int FoundXmps { get { return _imagesStatistics.FoundXmps; } set { _imagesStatistics.FoundXmps = value; } }
    public int FoundImages { get { return _imagesStatistics.FoundImages; } set { _imagesStatistics.FoundImages = value; } }
    public int SkippedXmps { get { return _imagesStatistics.SkippedXmps; } set { _imagesStatistics.SkippedXmps = value; } }
    public int SkippedImages { get { return _imagesStatistics.SkippedImages; } set { _imagesStatistics.SkippedImages = value; } }

    public IFileOperation FileOperation => _imagesStatistics.FileOperation;

    public void Log()
    {
        _imagesStatistics.Log();
        _directoriesStatistics.Log();
    }
}