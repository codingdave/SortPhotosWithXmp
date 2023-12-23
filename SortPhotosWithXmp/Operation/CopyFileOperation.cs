using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;
using SortPhotosWithXmp.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Operation;

public class CopyFileOperation : FileOperationBase
{
    private readonly IFile _file;

    internal CopyFileOperation(
        ILogger logger,
        IFile file,
        IDirectory directory,
        Action<FileAlreadyExistsError> handleError,
        bool isForce)
        : base(logger, directory, handleError, isForce) => _file = file ?? throw new ArgumentNullException(nameof(file));


    public override void ChangeFiles(IEnumerable<IImageFile> files, string targetPath)
    {
        void Errorhandler(string sourceFileName, string destFileName)
        {
            _logger.LogTrace($"CopyFileOperation({sourceFileName}, {destFileName});");
            _file.Move(sourceFileName, destFileName);
        };

        ChangeFiles(files, targetPath, Errorhandler);
    }
}