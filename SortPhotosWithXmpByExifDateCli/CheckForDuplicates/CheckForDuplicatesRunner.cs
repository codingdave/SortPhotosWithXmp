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
        private Dictionary<string, ImageHash> _imageHashes = new();
        private Dictionary<string, XmpHash> _xmpHashes = new();
        private readonly object _imageHashesLock = new object();
        private readonly object _xmpHashesLock = new object();
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
            var xmpDuplicatesGroup = _xmpHashes.Values.GroupBy(x => x.Hash).Where(g => g.Count() > 1);
            foreach (var duplicates in xmpDuplicatesGroup)
            {
                operation.HandleDuplicates(duplicates.Select(s => s.Filename));
            }
        }

        private void CreateSimilarityMap()
        {
            var imageHashList = _imageHashes.Values.ToList();
            // only doing for images. Not possible for xmps
            for (int i = 0; i < _imageHashes.Count; ++i)
            {
                var imageHash1 = imageHashList[i];
                for (int j = i + 1; j < _imageHashes.Count; ++j)
                {
                    var imageHash2 = imageHashList[j];
                    var percentageImageSimilarity = CompareHash.Similarity(imageHash1.Hash, imageHash2.Hash);
                    _imageSimilarity.Add((percentageImageSimilarity, imageHash1.Filename, imageHash2.Filename));
                }
            }
        }

        private void CreateHashes()
        {
            (_xmpHashes, _imageHashes) = _hashRepository.ReadHashes();

            void TickTimer(object? state)
            {
                List<XmpHash> xmpHashesList;
                lock (_xmpHashesLock)
                {
                    xmpHashesList = _xmpHashes.Values.ToList();
                }

                List<ImageHash> imageHashesList;
                lock (_imageHashesLock)
                {
                    imageHashesList = _imageHashes.Values.ToList();
                }

                _hashRepository.SaveHashes(xmpHashesList, imageHashesList);
            }

            using var saveStorageTimer = new Timer(TickTimer, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            var options = new EnumerationOptions() { RecurseSubdirectories = true };
            var files = Directory.EnumerateFiles(_imageDirectory, "*.*", options);
            Parallel.ForEach(files, CreateHash);

            void CreateHash(string path)
            {
                using var md5 = MD5.Create();
                if (path.EndsWith(".xmp"))
                {
                    if (_xmpHashes.ContainsKey(path))
                    {
                        _logger.LogInformation("Hash for {path} already calculated - Skipped", path);
                    }
                    else
                    {
                        _logger.LogInformation("Calculating hash for {path}", path);
                        CreateXmpHash(md5, path);
                    }
                }
                else
                {
                    if (_imageHashes.ContainsKey(path))
                    {
                        _logger.LogInformation("Hash for {path} already calculated - Skipped", path);
                    }
                    else
                    {
                        _logger.LogInformation("Calculating hash for {path}", path);
                        CreateImageHash(path);
                    }
                }
            }
        }

        private void CreateXmpHash(HashAlgorithm hashAlgorithm, string filename)
        {
            using var stream = File.OpenRead(filename);
            var hash = hashAlgorithm.ComputeHash(stream);
            lock (_xmpHashesLock)
            {
                _xmpHashes.Add(filename, new XmpHash(filename, hash, File.GetLastWriteTimeUtc(filename)));
            }
        }

        private void CreateImageHash(string filename)
        {
            try
            {
                using var imageStream = File.OpenRead(filename);
                var hash = _hashAlgorithm.Hash(imageStream);
                lock (_imageHashesLock)
                {
                    _imageHashes.Add(filename, new ImageHash(filename, hash, File.GetLastWriteTimeUtc(filename)));
                }
            }
            catch (UnknownImageFormatException e)
            {
                _logger.LogExceptionWarning(filename, e);
            }
            catch (Exception e)
            {
                _logger.LogExceptionError(e);
            }
        }
    }
}