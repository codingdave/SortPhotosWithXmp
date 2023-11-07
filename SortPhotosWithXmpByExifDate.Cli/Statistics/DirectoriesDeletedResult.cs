using Microsoft.Extensions.Logging;


using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics;

public class DirectoriesDeletedResult : IResult
{
    private readonly DeleteDirectoryOperation _deleteDirectoryOperation;
    private readonly ILogger _logger;

    public DirectoriesDeletedResult(ILogger logger, DeleteDirectoryOperation deleteDirectoryOperation)
    {
        _logger = logger;
        _deleteDirectoryOperation = deleteDirectoryOperation;
        ErrorCollection = new ErrorCollection.ErrorCollection(logger);
        SuccessfulCollection = new SuccessCollection();
    }

    public int DirectoriesFound { get; set; }
    public int DirectoriesDeleted { get; set; }

    public IReadOnlyErrorCollection ErrorCollection { get; }

    public IReadOnlySuccessCollection SuccessfulCollection { get; }

    public void Log()
    {
        _logger.LogInformation("-> {operation}. Found {DirectoriesFound}, deleted {DirectoriesDeleted} directories", _deleteDirectoryOperation.ToString(), DirectoriesFound, DirectoriesDeleted);

        foreach (var error in ErrorCollection.Errors)
        {
            _logger.LogError(error.ErrorMessage);
        }
    }
}