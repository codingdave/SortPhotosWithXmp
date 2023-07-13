using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.Operation
{
    public class MoveFileOperation : IFileOperation
    {
        private readonly ILogger _logger;

        internal MoveFileOperation(ILogger logger, bool force)
        {
            _logger = logger;
            IsChanging = force;
        }

        public bool IsChanging { get; }

        public void ChangeFile(string sourceFileName, string destFileName)
        {
            _logger.LogTrace($"File.Move({sourceFileName}, {destFileName});");
            if (IsChanging)
            {
                File.Move(sourceFileName, destFileName);
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