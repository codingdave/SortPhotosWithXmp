using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDate.Cli.Result
{
    internal class DuplicatesDeletedResult : IResult
    {
        public IPerformerCollection PerformerCollection => throw new NotImplementedException();

        public void Log(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}