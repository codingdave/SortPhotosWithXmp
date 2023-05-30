using System.Security.Cryptography;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.ColorSpaces;
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
        private readonly int _similarity;
        private readonly List<(ulong hash, string imagePath)> _imageHashes = new();
        private readonly List<(byte[] hash, string xmpPath)> _xmpHashes = new();
        private readonly List<(double similarity, string imagePath1, string imagePath2)> _imageSimilarity = new();
        readonly IImageHash _hashAlgorithm = new AverageHash();

        public CheckForDuplicatesRunner(ILogger<CommandLine> logger, string directory, bool force, int similarity = 100)
        {
            _logger = logger;
            _directory = directory;
            _force = force;
            _similarity = similarity;
        }

        public IStatistics Run(ILogger logger)
        {
            IOperation operation = null;
            try
            {
                CreateHashes();
                CreateSimilarityMap();
                HandlyMostSimilarImages(_similarity, operation);
            }
            catch (Exception e)
            {
                _logger.LogExceptionError(e);
            }

            return new DuplicatesDeletedStatistics();
        }

        private void HandlyMostSimilarImages(int similarity, IOperation operation)
        {
            // images
            _imageSimilarity.Sort(new ImageSimilarityComparer());
            foreach (var imageSimilarity in _imageSimilarity)
            {
                if (imageSimilarity.similarity > similarity)
                {
                    _logger.LogInformation("image '{image1}' and image '{image2}' are duplicates with a similarity score of {similarity}",
                                     imageSimilarity.imagePath1,
                                     imageSimilarity.imagePath2,
                                     imageSimilarity.similarity);
                    // operation();
                }
                else
                {
                    break;
                }
            }

            // xmps: only supports 100% match
            var xmpDuplicatesGroup = _xmpHashes.GroupBy(x => x.hash).Where(g => g.Count() > 1);
            foreach (IGrouping<byte[], (byte[] hash, string xmpPath)>? duplicates in xmpDuplicatesGroup)
            {
                _logger.LogInformation("Found {amount} xmp files that are a duplicates: {images}", duplicates.Count(), duplicates.Select(s => s.xmpPath));

            }
        }

        private void CreateSimilarityMap()
        {
            // only doing for images. Not possible for xmps
            for (int i = 0; i < _imageHashes.Count; ++i)
            {
                var (hash1, image1) = _imageHashes[i];
                for (int j = i + 1; j < _imageHashes.Count; ++j)
                {
                    var (hash2, image2) = _imageHashes[j];
                    var percentageImageSimilarity = CompareHash.Similarity(hash1, hash2);
                    _imageSimilarity.Add((percentageImageSimilarity, image1, image2));
                }
            }
        }

        private void CreateHashes()
        {

            void CreateHash(string path)
            {
                _logger.LogInformation("Calculating hash for {path}", path);

                using var md5 = MD5.Create();
                if (path.EndsWith(".xmp"))
                {
                    CreateXmpHash(md5, path);
                }
                else
                {
                    CreateImageHash(path);
                }
            }

            var files = Directory.EnumerateFiles(_directory, "*.*", new EnumerationOptions()
            {
                RecurseSubdirectories = true
            });

            Parallel.ForEach(files, CreateHash);
        }

        private void CreateXmpHash(HashAlgorithm hashAlgorithm, string path)
        {
            using var stream = File.OpenRead(path);
            var hash = hashAlgorithm.ComputeHash(stream);
            _xmpHashes.Add((hash, path));
        }

        private void CreateImageHash(string imagePath)
        {
            try
            {
                using var imageStream = File.OpenRead(imagePath);
                var hash = _hashAlgorithm.Hash(imageStream);
                _imageHashes.Add((hash, imagePath));
            }
            catch (UnknownImageFormatException e)
            {
                _logger.LogExceptionWarning(imagePath, e);
            }
            catch (Exception e)
            {
                _logger.LogExceptionError(e);
            }
        }
    }
}