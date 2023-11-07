using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public class DeleteDirectoryOperation : IOperation
    {
        private readonly ILogger _logger;
        private readonly IDirectory _directory;


        public DeleteDirectoryOperation(ILogger logger, IDirectory directory, bool force)
        {
            _logger = logger;
            _directory = directory;

            IsChanging = force;
            Statistics = new DirectoriesDeletedResult(logger, this);
        }

        public bool IsChanging { get; }
        public DirectoriesDeletedResult Statistics { get; }

        public void DeleteDirectory(string path)
        {
            if (IsChanging)
            {
                _logger.LogTrace("IDirectory.Delete({path});", path);
                _directory.Delete(path, false);
            }
            else
            {
                _logger.LogTrace("Ignoring IDirectory.Delete({path});", path);
            }

            // when we simulate, we still want to count
            Statistics.DirectoriesDeleted++;
        }


        public override string ToString()
        {
            var message = IsChanging ? "performing" : "simulating";
            message += " to delete the directory";
            return message;
        }
    }
}