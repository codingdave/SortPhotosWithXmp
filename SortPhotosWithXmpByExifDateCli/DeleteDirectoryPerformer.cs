using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli
{
    public class DeleteDirectoryOperation : IOperation
    {
        private readonly ILogger _logger;

        public DeleteDirectoryOperation(ILogger logger, bool force)
        {
            _logger = logger;
            IsChanging = force;
        }

        public bool IsChanging { get; }

        public void DeleteDirectory(string path)
        {
            if (IsChanging)
            {
                Directory.Delete(path, false);
            }
            else
            {
                _logger.LogInformation("Directory.Delete({path});", path);
            }
        }


        public override string ToString()
        {
            var message = IsChanging ? "performing" : "simulating";
            message += " directory deletion";
            return message;
        }
    }
}