using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Result
{
    internal class DuplicatesDeletedResult : IResult
    {
        public IReadOnlyErrorCollection ErrorCollection => throw new NotImplementedException();

        public IReadOnlyPerformerCollection PerformerCollection => throw new NotImplementedException();

        public void Log(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}