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
            #warning does a DeleteDirectoryOperation contain a DirectoriesDeletedResult or vive versa
            Result = new DirectoriesDeletedResult(logger, directory, _directory.GetCurrentDirectory(), force);
        }

        public bool IsChanging { get; }
        public DirectoriesDeletedResult Result { get; }

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
            Result.DirectoriesDeleted++;
        }


        public override string ToString()
        {
            var message = IsChanging ? "performing" : "simulating";
            message += " to delete the directory";
            return message;
        }
    }
}