using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class DeleteDirectoriesPerformer : IResult, IPerformer
{
    private readonly string _path;

    public DeleteDirectoriesPerformer(string path, DeleteFileOperation operation)
    {
        _path = path;
        ErrorCollection = new ErrorCollection.ErrorCollection();
        PerformerCollection = new PerformerCollection();
        _operation = operation;
    }

    public void Perform(ILogger logger)
    {
        logger.LogInformation(_operation.DirectoryStatistics.ToString());
        logger.LogDebug($"performing {_operation.GetType().FullName}, Force: {_operation.IsForce}");
        _operation.RecursivelyDeleteEmptyDirectories(_path);
        logger.LogInformation(_operation.DirectoryStatistics.ToString());
    }

    public IReadOnlyErrorCollection ErrorCollection { get; }

    public IReadOnlyPerformerCollection PerformerCollection { get; }

    private readonly DeleteFileOperation _operation;

    public void Log(ILogger logger)
    {
        foreach (var error in ErrorCollection.Errors)
        {
            logger.LogError(error.ErrorMessage);
        }
    }
}
