namespace SortPhotosWithXmpByExifDateCli;

public class ImagesAndXmpCopiedDirectoriesDeletedStatistics : IStatistics
{
    ImagesAndXmpFoundStatistics _imagesStatistics; 
    DirectoriesDeletedStatistics _directoriesStatistics;
    
    public ImagesAndXmpCopiedDirectoriesDeletedStatistics(
        ImagesAndXmpFoundStatistics imagesStatistics, 
        DirectoriesDeletedStatistics directoriesStatistics) => 
        (_imagesStatistics, _directoriesStatistics) = (imagesStatistics, directoriesStatistics);  
        
    public List<string> Errors { get; } = new List<string>();    

    public string PrintStatistics() => _imagesStatistics.PrintStatistics() + ", " + _directoriesStatistics.PrintStatistics();
}