namespace SortPhotosWithXmpByExifDateCli.Statistics;

public class ImagesAndXmpCopiedDirectoriesDeletedStatistics : IStatistics
{
    ImagesAndXmpFoundStatistics _imagesStatistics; 
    DirectoriesDeletedStatistics _directoriesStatistics;
    
    public ImagesAndXmpCopiedDirectoriesDeletedStatistics(
        ImagesAndXmpFoundStatistics imagesStatistics, 
        DirectoriesDeletedStatistics directoriesStatistics) => 
        (_imagesStatistics, _directoriesStatistics) = (imagesStatistics, directoriesStatistics); 

    public string PrintStatistics() => _imagesStatistics.PrintStatistics() + ", " + _directoriesStatistics.PrintStatistics();
}