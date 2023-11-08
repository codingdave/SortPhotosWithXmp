using ImageMagick;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class DirectoriesDeletedResult : IResult
{
    private readonly IDirectory _directory;
    private readonly string _sourceDirectory;

    public DirectoriesDeletedResult(IDirectory directory, string sourceDirectory)
    {
        _directory = directory;
        _sourceDirectory = sourceDirectory;
        ErrorCollection = new ErrorCollection.ErrorCollection();
        PerformerCollection = new PerformerCollection();
    }

    public void Perform(DeleteFileOperation operation)
    {
        operation.RecursivelyDeleteEmptyDirectories(_sourceDirectory);
    }

    public IReadOnlyErrorCollection ErrorCollection { get; }

    public IReadOnlyPerformerCollection PerformerCollection { get; }

    public void Log(ILogger logger)
    {
        foreach (var error in ErrorCollection.Errors)
        {
            logger.LogError(error.ErrorMessage);
        }
    }
}
