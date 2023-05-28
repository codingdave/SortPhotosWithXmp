using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli
{
    public class DeleteFileOperation : IFileOperation
    {
        private readonly ILogger _logger;

        internal DeleteFileOperation(ILogger logger, bool force)
        {
            _logger = logger;
            IsChanging = force;
        }

        public bool IsChanging { get; }

        public void ChangeFile(string path, string otherFile)
        {
            throw new NotImplementedException("The interface is not SOLID, it break the interface seggregation principle");
        }

        public void Delete(string path)
        {
            _logger.LogTrace($"File.Delete({path});");

            if (IsChanging)
            {
                File.Delete(path);
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