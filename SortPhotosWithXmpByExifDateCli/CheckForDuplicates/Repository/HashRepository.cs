using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates.Store
{
    internal class HashRepository
    {
        private readonly ILogger _logger;

        public HashRepository(ILogger logger)
        {
            _logger = logger;
        }

        internal (List<XmpHash> _xmpHashes, List<ImageHash> _imageHashes) ReadHashes()
        {
            _logger.LogWarning($"calling {nameof(ReadHashes)}");
            return (new List<XmpHash>(), new List<ImageHash>());
        }

        internal object? SaveHashes(List<XmpHash> xmpHashes, List<ImageHash> imageHashes)
        {
            _logger.LogWarning($"calling {nameof(SaveHashes)}");
            // var xmpDtoHashes = new List<XmpHash>();
            return null;
        }
    }
}