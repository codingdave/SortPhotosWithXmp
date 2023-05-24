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
       FileErrors = new MergedFileError(errorCollection1: _imagesStatistics.FileErrors, errorCollection2: _directoriesStatistics.FileErrors) ;
    }

    public IReadOnlyErrorCollection FileErrors { get; }
    public void Log()
    {
        _imagesStatistics.Log();
        _directoriesStatistics.Log();
    }
}