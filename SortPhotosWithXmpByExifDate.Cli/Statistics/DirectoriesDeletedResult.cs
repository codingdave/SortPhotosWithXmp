using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics;

public class DirectoriesDeletedResult : IResult
{
    private readonly DeleteDirectoryOperation _deleteDirectoryOperation;
    private readonly ILogger _logger;
    private readonly IDirectory _directory;
    private readonly string _sourceDirectory;

    public DirectoriesDeletedResult(ILogger logger, IDirectory directory, string sourceDirectory, bool force)
    {
        _logger = logger;
        _directory = directory;
        _sourceDirectory = sourceDirectory;
        _deleteDirectoryOperation = new DeleteDirectoryOperation(logger, _directory, force);
        ErrorCollection = new ErrorCollection.ErrorCollection(logger);
        SuccessfulCollection = new SuccessCollection();        
    }

    public void Perform()
    {
        // DeleteEmptyDirectoryRunner?
        Helpers.RecursivelyDeleteEmptyDirectories(_logger, _directory, _sourceDirectory, _deleteDirectoryOperation);
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