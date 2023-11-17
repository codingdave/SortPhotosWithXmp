using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Result
{
    internal class DuplicatesDeletedResult : IResult
    {
        public IPerformerCollection Performers => throw new NotImplementedException();

        public void Log(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}