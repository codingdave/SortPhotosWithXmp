using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli
{
    public class MoveOperationPerformer : IFileOperation
    {
        private readonly ILogger _logger;

        public MoveOperationPerformer(ILogger logger, bool force)
        {
            _logger = logger;
            IsChanging = force;
        }

        public bool IsChanging { get; }

        public void ChangeFile(string sourceFileName, string destFileName)
        {
            if (IsChanging)
            {
                File.Move(sourceFileName, destFileName);
            }
            else
            {
                _logger.LogInformation($"File.Move({sourceFileName}, {destFileName});");
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