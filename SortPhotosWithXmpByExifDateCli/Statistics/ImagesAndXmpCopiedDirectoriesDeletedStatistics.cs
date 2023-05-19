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
       ReadOnlyFileError = new MergedFileError(errorCollection1: _imagesStatistics.ReadOnlyFileError, errorCollection2: _directoriesStatistics.ReadOnlyFileError) ;
    }

    public IReadOnlyFileError ReadOnlyFileError { get; } 
    public string PrintStatistics() => _imagesStatistics.PrintStatistics() + Environment.NewLine + _directoriesStatistics.PrintStatistics();
}