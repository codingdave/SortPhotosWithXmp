using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;

using SystemInterface.IO;


namespace SortPhotosWithXmp.Operation;

public class Operations
{
    private readonly IFile _file;
    private readonly IDirectory _directory;

    private static void Errorhandler(FileAlreadyExistsError e)
    {
        throw new NotImplementedException();
    }

    private readonly bool _isForce;

    // when we have an error, we want to copy
    private readonly bool _isCopyingEnforced;

    public CopyFileOperation CopyFileOperation { get; }

    public MoveFileOperation MoveFileOperation { get; }

    public DeleteFileOperation DeleteFileOperation { get; }

    public Operations(ILogger logger, IFile file, IDirectory directory, bool isForce, bool isCopyingEnforced)
    {
        _file = file;
        _directory = directory;
        _isForce = isForce;
        _isCopyingEnforced = isCopyingEnforced;

#warning copy and move should HAVE not ARE a directory operator
        CopyFileOperation = new CopyFileOperation(logger, _file, _directory, Errorhandler, _isCopyingEnforced);
        MoveFileOperation = new MoveFileOperation(logger, _file, _directory, Errorhandler, _isCopyingEnforced);
        DeleteFileOperation = new DeleteFileOperation(logger, _file, _directory, _isForce);
    }
}