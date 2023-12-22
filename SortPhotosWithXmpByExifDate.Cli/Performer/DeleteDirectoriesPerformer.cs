using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Operation;

namespace SortPhotosWithXmpByExifDate.Performer;

public class DeleteDirectoriesPerformer : IPerformer
{
    private readonly string _path;

    public DeleteDirectoriesPerformer(string path, DeleteFileOperation operation)
    {
        _path = path;
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

    public IPerformerCollection PerformerCollection { get; }

    private readonly DeleteFileOperation _operation;
}
