using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli
{
    public class CopyOperationPerformer : IFileOperation
    {
        private readonly ILogger _logger;
        
        public CopyOperationPerformer(ILogger logger, bool force)
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
                _logger.LogInformation($"File.Copy({sourceFileName}, {destFileName});");
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