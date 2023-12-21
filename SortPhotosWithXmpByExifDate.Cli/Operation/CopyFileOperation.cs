using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operation;

public class CopyFileOperation : FileOperationBase
{
    private readonly IFile _file;

    internal CopyFileOperation(
        ILogger logger,
        IFile file,
        IDirectory directory,
        Action<FileAlreadyExistsError> handleError,
        bool isForce)
        : base(logger, directory, handleError, isForce)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }


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