using Microsoft.Extensions.Logging;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public class DeleteFileOperation : IFileOperation
    {
        private readonly ILogger _logger;
        private readonly IFile _file;


        internal DeleteFileOperation(ILogger logger, IFile file, bool force)
        {
            _logger = logger;
            _file = file;

            IsChanging = force;
        }

        public bool IsChanging { get; }

        public void ChangeFile(string path, string otherFile)
        {
            throw new NotImplementedException("The interface is not SOLID, it break the interface seggregation principle");
        }

        public void Delete(string path)
        {
            _logger.LogTrace($"IFile.Delete '{path}';");
            if (IsChanging)
            {
                _file.Delete(path);
            }
        }

        public override string ToString()
        {
            var message = IsChanging ? "performing" : "simulating";
            message += " delete";
            return message;
        }
    }
}