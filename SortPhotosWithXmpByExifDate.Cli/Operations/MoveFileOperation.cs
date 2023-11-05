using Microsoft.Extensions.Logging;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public class MoveFileOperation : IFileOperation
    {
        private readonly ILogger _logger;
        private readonly IFile _fileWrapper;


        internal MoveFileOperation(ILogger logger, IFile file, bool force)
        {
            _logger = logger;
            _fileWrapper = file;

            IsChanging = force;
        }

        public bool IsChanging { get; }

        public void ChangeFile(string sourceFileName, string destFileName)
        {
            _logger.LogTrace($"IFile.Move({sourceFileName}, {destFileName});");
            if (IsChanging)
            {
                _fileWrapper.Move(sourceFileName, destFileName);
            }
        }

        public override string ToString()
        {
            var message = IsChanging ? "performing" : "simulating";
            message += " move";
            return message;
        }
    }
}