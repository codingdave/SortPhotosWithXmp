using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli
{
    public class CopyFileOperation : IFileOperation
    {
        private readonly ILogger _logger;
        
        internal CopyFileOperation(ILogger logger, bool force)
        {
            _logger = logger;
            IsChanging = force;
        }

        public bool IsChanging { get; }

        public void ChangeFile(string sourceFileName, string destFileName)
        {
            if (IsChanging)
            {
                File.Copy(sourceFileName, destFileName);
            }
            else
            {
                _logger.LogTrace($"File.Copy({sourceFileName}, {destFileName});");
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