using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Statistics
{
    internal class DuplicatesDeletedResult : IResult
    {
        private readonly ILogger _logger;

        public DuplicatesDeletedResult(ILogger logger) => _logger = logger;

        public IReadOnlyErrorCollection ErrorCollection => throw new NotImplementedException();

        public IReadOnlySuccessCollection SuccessfulCollection => throw new NotImplementedException();

        public void Log()
        {
            throw new NotImplementedException();
        }
    }
}