using System.Security.Cryptography;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.CheckForDuplicates.Store;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;
using SortPhotosWithXmpByExifDateCli.Operation;
using SortPhotosWithXmpByExifDateCli.Statistics;

namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    internal class CheckForDuplicatesRunner : IRun
    {
        private readonly ILogger<CommandLine> _logger;
        private readonly string _imageDirectory;
        private readonly int _similarity;
        private Dictionary<string, ImageHash> _imageHashes = new();
        private Dictionary<string, XmpHash> _xmpHashes = new();
        private readonly object _imageHashesLock = new();
        private readonly object _xmpHashesLock = new();
        private readonly HashRepository _hashRepository;
        private readonly bool _force;
        private readonly List<(double similarity, string imagePath1, string imagePath2)> _imageSimilarity = new();
        private readonly IImageHash _hashAlgorithm = new AverageHash();

        public CheckForDuplicatesRunner(ILogger<CommandLine> logger, string imageDirectory, HashRepository repository, bool force, int similarity = 100)
        {
            _logger = logger;
            _imageDirectory = imageDirectory;
            _similarity = similarity;
            _hashRepository = repository;
            _force = force;
        }

        public IStatistics Run(ILogger logger)
        {
            try
            {
#warning TODO
                // 1. before we search for duplicate images and xmps we check for lonely xmps
                // 2. now we are sure to find pairs <img,xmp> or <img, null>, so we create all the pairs
                // 3. work on the pairs and create image and xmp hashes and the similarity:
                // 3.1 when we have a similarity on the image and an equality for the xmp we are sure about the duplicate
                // 3.2 when we have a different image, the xmp should also be different. What if it is not?
                // 3.3 when we have a different xmp but the same image, the xmp might be a different development of the same file
                DeleteLonelyXmps();
                // they could be <img, null>
                CreateImageXmpPairs();
                CreateHashes();
                CreateSimilarityMap(_similarity);
                HandleMostSimilarImages();
            }
            catch (Exception e)
            {
                _logger.LogExceptionError(e);
            }

            return new DuplicatesDeletedStatistics(_logger);
        }

        private void DeleteLonelyXmps()
        {
            throw new NotImplementedException();
        }

        private void CreateImageXmpPairs()
        {
            throw new NotImplementedException();
        }

        private void HandleMostSimilarImages()
        {
            // images
            _imageSimilarity.Sort(new ImageSimilarityComparer());
            foreach (var (similarity, imagePath1, imagePath2) in _imageSimilarity)
            {
                DeleteDuplicateImages(imagePath1, imagePath2, similarity);
            }

            // xmps: only supports 100% match
            var xmpDuplicatesGroup = _xmpHashes.Values.GroupBy(x => x.Hash).Where(g => g.Count() > 1);
            foreach (var duplicates in xmpDuplicatesGroup)
            {
                DeleteDuplicateXmps(duplicates.Select(s => s.Filename));
            }
        }

         public void DeleteDuplicateImages(string imagePath1, string imagePath2, double similarity)
        {
            #warning TODO
            // deletion of images - which one shall we delete?
            // imagine one of them has a descriptive filename, the other does not
            
            // we should at first copy them all next to each other to evaluate in the duplicate directory
            _logger.LogInformation(
                "image '{image1}' and image '{image2}' are duplicates with a similarity score of {similarity}",
                imagePath1,
                imagePath2,
                similarity);

            if (_force)
            {
            }
        }

        public void DeleteDuplicateXmps(IEnumerable<string> enumerable)
        {
            #warning TODO

            // deletion of duplicate xmps is critical:
            // - We need to delete the one that is not next to the corresponding image.
            // - if there are more locations with existing images the images would need to match as well and then we could delete the image with its xmp.
            // - -> xmps should always be handled as a tuple: (image, xmp) to allow for that

            // we should at first copy them all next to each other to evaluate in the duplicate directory
            var list = enumerable.ToList();
            _logger.LogInformation("Found {amount} xmp files that are duplicates: {images}",
                                   list.Count,
                                   list);
            if (_force)
            {
            }
        }

        private void CreateSimilarityMap(int similarity)
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
                    if (percentageImageSimilarity >= similarity)
                    {
                        _imageSimilarity.Add((percentageImageSimilarity, imageHash1.Filename, imageHash2.Filename));
                    }
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
                        _logger.LogTrace("Hash for {path} already calculated - Skipped", path);
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
                        _logger.LogTrace("Hash for {path} already calculated - Skipped", path);
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