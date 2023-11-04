using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    internal class DuplicatesDeletedStatistics : IStatistics
    {
        private readonly ILogger _logger;

        public DuplicatesDeletedStatistics(ILogger logger)
        {
            _logger = logger;
        }

        public IReadOnlyErrorCollection FileErrors => throw new NotImplementedException();

        public void Log()
        {
#if DEBUG
            _logger.LogWarning("NotImplemented");
#else
            throw new NotImplementedException();
#endif
        }
    }
}