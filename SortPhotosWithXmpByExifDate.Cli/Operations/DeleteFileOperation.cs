using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

using SortPhotosWithXmpByExifDate.Cli.Repository;

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

        public void ChangeFiles(IEnumerable<IImageFile> files, string targetPath)
        {
            throw new NotImplementedException("The interface is not SOLID, it break the interface seggregation principle");
        }

        public void Delete(string path)
        {
            if (IsChanging)
            {
                _logger.LogTrace($"IFile.Delete '{path}';");
                _file.Delete(path);
            }
            else
            {
                _logger.LogTrace($"Ignoring IFile.Delete '{path}';");
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