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
       ErrorCollection = new MergedErrorCollection(errorCollection1: _imagesStatistics.ErrorCollection, errorCollection2: _directoriesStatistics.ErrorCollection) ;
    }

    public IReadOnlyErrorCollection ErrorCollection { get; } 
    public string PrintStatistics() => _imagesStatistics.PrintStatistics() + ", " + _directoriesStatistics.PrintStatistics();
}