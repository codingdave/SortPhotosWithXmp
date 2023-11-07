using Microsoft.Extensions.Logging;
using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public class CopyFileOperation : IFileOperation
    {
        private readonly ILogger _logger;
        private readonly IFile _file;


        internal CopyFileOperation(
            ILogger logger,
            IFile file, 
            bool force)
        {
            _logger = logger;
            _file = file;
            IsChanging = force;
        }

        public bool IsChanging { get; }

        public void ChangeFile(string sourceFileName, string destFileName)
        {
            _logger.LogTrace($"IFile.Copy({sourceFileName}, {destFileName});");
            if (IsChanging)
            {
                _file.Copy(sourceFileName, destFileName);
            }
        }

        public override string ToString()
        {
            var message = IsChanging ? "performing" : "simulating";
            message += " copy";
            return message;
        }
    }
}