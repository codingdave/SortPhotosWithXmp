using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;

using SystemInterface.IO;


namespace SortPhotosWithXmp.Operation;

public class Operations
{
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

    public IFile FileWrapper { get; }

    public IDirectory DirectoryWrapper { get; }

    public Operations(ILogger logger, IFile fileWrapper, IDirectory directoryWrapper, bool isForce, bool isCopyingEnforced)
    {
        FileWrapper = fileWrapper;
        DirectoryWrapper = directoryWrapper;
        _isForce = isForce;
        _isCopyingEnforced = isCopyingEnforced;

#warning copy and move should HAVE not ARE a directory operator
        CopyFileOperation = new CopyFileOperation(logger, FileWrapper, DirectoryWrapper, Errorhandler, _isCopyingEnforced);
        MoveFileOperation = new MoveFileOperation(logger, FileWrapper, DirectoryWrapper, Errorhandler, _isCopyingEnforced);
        DeleteFileOperation = new DeleteFileOperation(logger, FileWrapper, DirectoryWrapper, _isForce);
    }
}