using ImageMagick;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class DirectoriesDeletedResult : IResult
{
    private readonly ILogger _logger;
    private readonly IDirectory _directory;
    private readonly string _sourceDirectory;

    public DirectoriesDeletedResult(ILogger logger, IDirectory directory, string sourceDirectory)
    {
        _logger = logger;
        _directory = directory;
        _sourceDirectory = sourceDirectory;
        ErrorCollection = new ErrorCollection.ErrorCollection(logger);
        SuccessfulCollection = new SuccessCollection();
    }

    public void Perform(DeleteFileOperation operation)
    {
        operation.RecursivelyDeleteEmptyDirectories(_sourceDirectory);
    }

    public IReadOnlyErrorCollection ErrorCollection { get; }

    public IReadOnlySuccessCollection SuccessfulCollection { get; }

    public void Log()
    {
        foreach (var error in ErrorCollection.Errors)
        {
            _logger.LogError(error.ErrorMessage);
        }
    }
}
