using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    internal class CheckForDuplicatesRunner : IRun
    {
        private readonly ILogger<CommandLine> _logger;
        private readonly string _directory;
        private readonly bool _force;

        public CheckForDuplicatesRunner(ILogger<CommandLine> logger, string directory, bool force)
        {
            _logger = logger;
            _directory = directory;
            _force = force;
        }

        public IStatistics Run(ILogger logger)
        {
            try
            {
                var basePath = "/run/media/david/1.44.1-42962/";
                var f1 = basePath + "Photos/Fotos/20150106/dsc_1491.JPG";
                var f2 = basePath + "Photos/Fotos/20150601/dsc_1491.JPG";
                // var hash1 = MD5.Create().ComputeHash(File.Open(f1, FileMode.Open));
                // var hash2 = MD5.Create().ComputeHash(File.Open(f2, FileMode.Open));

                var hashAlgorithm = new AverageHash();
                // or one of the other available algorithms:
                // var hashAlgorithm = new DifferenceHash();
                // var hashAlgorithm = new PerceptualHash();

                using var stream1 = File.OpenRead(f1);
                ulong hash1 = hashAlgorithm.Hash(stream1);

                using var stream2 = File.OpenRead(f2);
                ulong hash2 = hashAlgorithm.Hash(stream2);

                var percentageImageSimilarity = CompareHash.Similarity(hash1, hash2);

                // 2015/01/06/dsc_1491.JPG and 2015/06/01/dsc_1491.JPG 
                // Run(new FixExifDateByOffset(directory, (TimeSpan)offset, force));
                // throw new NotImplementedException();

            }
            catch (Exception e)
            {
                _logger.LogExceptionError(e);
            }

            return new DuplicatesDeletedStatistics();
        }
    }
}