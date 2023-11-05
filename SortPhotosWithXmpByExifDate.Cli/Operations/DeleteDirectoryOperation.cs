using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public class DeleteDirectoryOperation : IOperation
    {
        private readonly ILogger _logger;
        private readonly IDirectory _directoryWrapper;


        public DeleteDirectoryOperation(ILogger logger, IDirectory directoryWrapper, bool force)
        {
            _logger = logger;
            _directoryWrapper = directoryWrapper;

            IsChanging = force;
            Statistics = new DirectoriesDeletedStatistics(logger, this);
        }

        public bool IsChanging { get; }
        public DirectoriesDeletedStatistics Statistics { get; }

        public void DeleteDirectory(string path)
        {
            _logger.LogTrace("IDirectory.Delete({path});", path);

            if (IsChanging)
            {
                _directoryWrapper.Delete(path, false);
            }

            // when we simulate, we still want to count
            Statistics.DirectoriesDeleted++;
        }


        public override string ToString()
        {
            var message = IsChanging ? "performing" : "simulating";
            message += " directory deletion";
            return message;
        }
    }
}