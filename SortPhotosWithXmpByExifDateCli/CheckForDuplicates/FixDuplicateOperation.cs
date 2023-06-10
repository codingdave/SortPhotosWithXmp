using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    public class FixDuplicateOperation : IFixDuplicatesOperation
    {
        private readonly ILogger _logger;

        public FixDuplicateOperation(ILogger logger, bool _force)
        {
            _logger = logger;
            IsChanging = _force;
        }

        public bool IsChanging { get; }

        public void HandleDuplicates(string imagePath1, string imagePath2, double similarity)
        {
            _logger.LogInformation(
                "image '{image1}' and image '{image2}' are duplicates with a similarity score of {similarity}",
                imagePath1,
                imagePath2,
                similarity);
#warning Move items into duplicates directory if force option is set
        }

        public void HandleDuplicates(IEnumerable<string> enumerable)
        {
            var list = enumerable.ToList();
            _logger.LogInformation("Found {amount} xmp files that are a duplicates: {images}",
                                   list.Count,
                                   list);
#warning Move items into duplicates directory if force option is set

        }
    }
}