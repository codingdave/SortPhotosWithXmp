using SortPhotosWithXmpByExifDateCli.ErrorCollection;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    internal class DuplicatesDeletedStatistics : IStatistics
    {
        public IReadOnlyErrorCollection FileErrors => throw new NotImplementedException();

        public void Log()
        {
            throw new NotImplementedException();
        }
    }
}