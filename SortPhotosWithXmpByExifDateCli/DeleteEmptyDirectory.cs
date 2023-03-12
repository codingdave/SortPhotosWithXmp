namespace SortPhotosWithXmpByExifDateCli;

public class DeleteEmptyDirectory : IRun 
{
    private DirectoryInfo _directory;
    private bool _force;
    public DeleteEmptyDirectory(DirectoryInfo directory, bool force) => 
        (_directory, _force) = (directory, force);
    public IStatistics Run()
    {
        var statistics = new DirectoriesDeletedStatistics(_force);
        Helpers.RecursivelyDeleteEmptyDirectories(_directory, statistics, _force);
        return statistics;
    }
}