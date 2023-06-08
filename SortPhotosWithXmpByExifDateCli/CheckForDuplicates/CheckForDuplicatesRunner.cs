using System.Security.Cryptography;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.CheckForDuplicates.Store;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    internal class CheckForDuplicatesRunner : IRun
    {
        private readonly ILogger<CommandLine> _logger;
        private readonly string _imageDirectory;
        private readonly bool _force;
        private readonly int _similarity;
        private List<ImageHash> _imageHashes = new();
        private List<XmpHash> _xmpHashes = new();
        private readonly HashRepository _hashRepository;
        private readonly List<(double similarity, string imagePath1, string imagePath2)> _imageSimilarity = new();
        readonly IImageHash _hashAlgorithm = new AverageHash();

        public CheckForDuplicatesRunner(ILogger<CommandLine> logger, string imageDirectory, HashRepository repository, bool force, int similarity = 100)
        {
            _logger = logger;
            _imageDirectory = imageDirectory;
            _force = force;
            _similarity = similarity;
            _hashRepository = repository;
        }

        public IStatistics Run(ILogger logger)
        {
            IFixDuplicatesOperation operation = new FixDuplicateOperation(_logger, _force);
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

        private void HandlyMostSimilarImages(int similarity, IFixDuplicatesOperation operation)
        {
            // images
            _imageSimilarity.Sort(new ImageSimilarityComparer());
            foreach (var imageSimilarity in _imageSimilarity)
            {
                if (imageSimilarity.similarity > similarity)
                {
                    operation.HandleDuplicates(imageSimilarity.imagePath1, imageSimilarity.imagePath2, imageSimilarity.similarity);
                }
                else
                {
                    break;
                }
            }

            // xmps: only supports 100% match
            var xmpDuplicatesGroup = _xmpHashes.GroupBy(x => x.Hash).Where(g => g.Count() > 1);
            foreach (var duplicates in xmpDuplicatesGroup)
            {
                operation.HandleDuplicates(duplicates.Select(s => s.Filename));
            }
        }

        private void CreateSimilarityMap()
        {
            // only doing for images. Not possible for xmps
            for (int i = 0; i < _imageHashes.Count; ++i)
            {
                var imageHash1 = _imageHashes[i];
                for (int j = i + 1; j < _imageHashes.Count; ++j)
                {
                    var imageHash2 = _imageHashes[j];
                    var percentageImageSimilarity = CompareHash.Similarity(imageHash1.Hash, imageHash2.Hash);
                    _imageSimilarity.Add((percentageImageSimilarity, imageHash1.Filename, imageHash2.Filename ));
                }
            }
        }

        private void CreateHashes()
        {
            (_xmpHashes, _imageHashes) = _hashRepository.ReadHashes();

            void TickTimer(object? state)
            {
                _hashRepository.SaveHashes(_xmpHashes, _imageHashes);
            }

            using var saveStorageTimer = new Timer(new TimerCallback(TickTimer), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            var options = new EnumerationOptions() { RecurseSubdirectories = true };
            var files = Directory.EnumerateFiles(_imageDirectory, "*.*", options);
            Parallel.ForEach(files, CreateHash);

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
        }

        private void CreateXmpHash(HashAlgorithm hashAlgorithm, string xmpPath)
        {
            using var stream = File.OpenRead(xmpPath);
            var hash = hashAlgorithm.ComputeHash(stream);
            _xmpHashes.Add(new XmpHash(xmpPath, hash, File.GetLastWriteTimeUtc(xmpPath)));
        }

        private void CreateImageHash(string imagePath)
        {
            try
            {
                using var imageStream = File.OpenRead(imagePath);
                var hash = _hashAlgorithm.Hash(imageStream);
                _imageHashes.Add(new ImageHash(imagePath, hash, File.GetLastWriteTimeUtc(imagePath)));
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