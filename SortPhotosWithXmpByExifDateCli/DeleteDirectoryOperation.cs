using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli
{
    public class DeleteDirectoryOperation : IOperation
    {
        private readonly ILogger _logger;
        private readonly DirectoriesDeletedStatistics _statistics;

        public DeleteDirectoryOperation(ILogger logger, bool force)
        {
            _logger = logger;
            IsChanging = force;

            _statistics = new DirectoriesDeletedStatistics(logger, this);
        }

        public bool IsChanging { get; }
        public DirectoriesDeletedStatistics Statistics { get; internal set; }

        public void DeleteDirectory(string path)
        {
            if (IsChanging)
            {
                Directory.Delete(path, false);
            }
            else
            {
                _logger.LogTrace("Directory.Delete({path});", path);
            }
            _statistics.DirectoriesDeleted++;
        }


        public override string ToString()
        {
            var message = IsChanging ? "performing" : "simulating";
            message += " directory deletion";
            return message;
        }
    }
}