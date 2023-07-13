using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli.Operation
{
    public class DeleteDirectoryOperation : IOperation
    {
        private readonly ILogger _logger;

        public DeleteDirectoryOperation(ILogger logger, bool force)
        {
            _logger = logger;
            IsChanging = force;
            Statistics = new DirectoriesDeletedStatistics(logger, this);
        }

        public bool IsChanging { get; }
        public DirectoriesDeletedStatistics Statistics { get; }

        public void DeleteDirectory(string path)
        {
            _logger.LogTrace("Directory.Delete({path});", path);

            if (IsChanging)
            {
                Directory.Delete(path, false);
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