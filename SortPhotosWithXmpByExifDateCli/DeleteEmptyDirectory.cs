namespace SortPhotosWithXmpByExifDateCli;

public class DeleteEmptyDirectory : IRun 
{
    DirectoryInfo _directory;
    public DeleteEmptyDirectory(DirectoryInfo directory) => _directory = directory;
    public IStatistics Run()
    {
        var statistics = new DirectoriesDeletedStatistics();
        Helpers.RecursivelyDeleteEmptyDirectories(_directory, statistics);
        return statistics;
    }
}