using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class ImagesAndXmpCopiedDirectoriesDeletedStatistics : IStatistics
{
    readonly ImagesAndXmpFoundStatistics _imagesStatistics;
    readonly DirectoriesDeletedStatistics _directoriesStatistics;

    public ImagesAndXmpCopiedDirectoriesDeletedStatistics(
        ImagesAndXmpFoundStatistics imagesStatistics,
        DirectoriesDeletedStatistics directoriesStatistics)
    {
        (_imagesStatistics, _directoriesStatistics) = (imagesStatistics, directoriesStatistics);
       FileError = new MergedFileError(errorCollection1: _imagesStatistics.FileError, errorCollection2: _directoriesStatistics.FileError) ;
    }

    public IReadOnlyFileError FileError { get; }
    public void Log(ILogger logger)
    {
        _imagesStatistics.Log(logger);
        _directoriesStatistics.Log(logger);
    }
}