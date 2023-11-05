using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics;

public class DirectoriesDeletedStatistics : IStatistics
{
    private readonly DeleteDirectoryOperation _deleteDirectoryOperation;
    private readonly ILogger _logger;

    public DirectoriesDeletedStatistics(ILogger logger, DeleteDirectoryOperation deleteDirectoryOperation)
    {
        _logger = logger;
        _deleteDirectoryOperation = deleteDirectoryOperation;
        FileErrors = new ErrorCollection.ErrorCollection(logger);
    }

    public int DirectoriesFound { get; set; }
    public int DirectoriesDeleted { get; set; }

    public IReadOnlyErrorCollection FileErrors { get; } 

    public void Log()
    {
        _logger.LogInformation("-> {operation}. Found {DirectoriesFound}, deleted {DirectoriesDeleted} directories", _deleteDirectoryOperation.ToString(), DirectoriesFound, DirectoriesDeleted);

        foreach (var error in FileErrors.Errors)
        {
            _logger.LogError(error.ErrorMessage);
        }
    }
}