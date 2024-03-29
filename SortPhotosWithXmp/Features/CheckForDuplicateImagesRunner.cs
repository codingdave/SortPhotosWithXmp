using System.Security.Cryptography;

using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Repository;
using SortPhotosWithXmp.Result;
using SortPhotosWithXmp.Wrappers;

using SystemWrapper.IO;

using Configuration = SortPhotosWithXmp.Core.Configuration;

namespace SortPhotosWithXmp.Features
{
    public class CheckForDuplicateImagesRunner : IRun
    {
        private readonly ILogger<LoggerContext> _logger;
        private readonly int _similarity;
        private readonly HashRepository _hashRepository;
        private readonly FileScanner _fileScanner;
        private readonly List<(double similarity, FileVariations first, FileVariations second)> _imageSimilarity = new();
        private readonly IImageHash _hashAlgorithm = new AverageHash();

        public bool IsForce { get; }

        public CheckForDuplicateImagesRunner(ILogger<LoggerContext> logger, FileScanner fileScanner, bool isForce, int similarity = 100)
        {
            _logger = logger;
            _similarity = similarity;
            _hashRepository = new HashRepository(logger, Configuration.GetBasePath(), fileScanner.FileWrapper);
            _fileScanner = fileScanner;
            IsForce = isForce;
        }

        public IResult Run(ILogger logger)
        {
            try
            {
                throw new NotImplementedException();

                // 0. Work only on non-lonely xmps
                // REM - 1. before we search for duplicate images and xmps we check for lonely xmps
                // REM - 2. now we are sure to find pairs <img,xmp> or <img, null>
                // 3. work on the pairs and create image and xmp hashes and the similarity:
                // 3.1 when we have a similarity on the image and an equality for the xmp we are sure about the duplicate
                // 3.2 when we have a different image, the xmp should also be different. What if it is not?
                // 3.3 when we have a different xmp but the same image, the xmp might be a different development of the same file
                CreateHashes();
                CreateSimilarityMap(_fileScanner.FilenameMap.Values, _similarity);
                HandleMostSimilarImages();
            }
            catch (Exception e)
            {
                _logger.LogExceptionError(e);
            }

            return new DuplicatesDeletedResult();
        }

        private bool LonelyXmpsExist()
        {
            return !_fileScanner.LonelySidecarFiles.Any();
        }

        private void HandleMostSimilarImages()
        {
            // images
            _imageSimilarity.Sort(new FileVariationSimilarityComparer());
            foreach (var (similarity, first, second) in _imageSimilarity)
            {
                DeleteDuplicateImages(first, second, similarity);
            }

            // xmps: only supports 100% match if its an edit of the same image data. 
            // Do not check edits across different data.
            foreach (var hashedSidecars in _fileScanner.FilenameMap.Values)
            {
                var xmpDuplicatesGroup = hashedSidecars.SidecarFiles.Cast<SidecarFileHash>().GroupBy(x => x.Hash).Where(g => g.Count() > 1);
                foreach (var duplicates in xmpDuplicatesGroup)
                {
                    DeleteDuplicateXmps(duplicates.Select(s => s.OriginalFilename));
                }
            }
        }

        public void DeleteDuplicateImages(FileVariations first, FileVariations second, double similarity)
        {
            // deletion of images - which one shall we delete?
            // imagine one of them has a descriptive filename, the other does not
            // we should at first copy them all next to each other to evaluate in the duplicate directory
            _logger.LogInformation(
                "image '{first}' and image '{second}' are duplicates with a similarity score of {similarity}",
                first,
                second,
                similarity);

            if (IsForce)
            {
            }

            throw new NotImplementedException("");
        }

        public void DeleteDuplicateXmps(IEnumerable<string> enumerable)
        {
            // deletion of duplicate xmps is critical:
            // - We need to delete the one that is not next to the corresponding image.
            // - if there are more locations with existing images the images would need to match as well and then we could delete the image with its xmp.
            // - -> xmps should always be handled as a tuple: (image, xmp) to allow for that
            // we should at first copy them all next to each other to evaluate in the duplicate directory
            var list = enumerable.ToList();
            _logger.LogInformation("Found {amount} xmp files that are duplicates: {images}",
                                   list.Count,
                                   list);
            if (IsForce)
            {
            }

            throw new NotImplementedException();
        }

        private void CreateSimilarityMap(IEnumerable<FileVariations> hashedImages, int similarity)
        {
            var hashedList = hashedImages.ToList();

            // only doing for images. Not possible for xmps
            for (var i = 0; i < hashedList.Count; ++i)
            {
                if (hashedList[i].Data is ImageFileHash imageHash1)
                {
                    for (var j = i + 1; j < hashedList.Count; ++j)
                    {
                        if (hashedList[j].Data is ImageFileHash imageHash2)
                        {
                            var percentageImageSimilarity = CompareHash.Similarity(imageHash1.Hash, imageHash2.Hash);
                            if (percentageImageSimilarity >= similarity)
                            {
                                _imageSimilarity.Add((percentageImageSimilarity, hashedList[i], hashedList[j]));
                            }
                        }
                    }
                }
            }
        }

        private void CreateHashes()
        {
            var fileVariations = _hashRepository.ReadRepository();

            void SaveStorageTick(object? state)
            {
                _hashRepository.SaveRepository(_fileScanner.FilenameMap.Values.ToList());
            }

            using var saveStorageTimer = new Timer(SaveStorageTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            var result = Parallel.ForEach(_fileScanner.FilenameMap.Values, CreateHashes);
            if (!result.IsCompleted)
            {
                _logger.LogError($"Parallel.ForEach failed - LowestBreakIteration: {result.LowestBreakIteration}, {result}");
            }

            void CreateHashes(FileVariations variations)
            {
                if (variations.Data is not null &&
                    (variations.Data is not IPerceptualHash || variations.Data.IsModified))
                {
                    variations.Data = CreateImageHash(variations.Data.OriginalFilename);
                }

                if (variations.SidecarFiles.Any(x => x is not IHash || x.IsModified))
                {
                    var sidecarFiles = new List<SidecarFileHash>();
                    foreach (var sidecarFile in variations.SidecarFiles)
                    {
                        if (sidecarFile is SidecarFileHash hashedVariation && !sidecarFile.IsModified)
                        {
                            sidecarFiles.Add(hashedVariation);
                        }
                        else
                        {
#warning use the FileScanner
                            using var md5 = MD5.Create();
                            sidecarFiles.Add(CreateXmpHash(md5, sidecarFile.OriginalFilename));
                        }
                    }
                }
            }
        }

        private SidecarFileHash CreateXmpHash(HashAlgorithm hashAlgorithm, string filename)
        {
            using var stream = _fileScanner.FileWrapper.OpenRead(filename).StreamInstance;
            var hash = hashAlgorithm.ComputeHash(stream);
            return new SidecarFileHash(filename, hash, _fileScanner.FileWrapper.GetLastWriteTimeUtc(filename), _fileScanner.FileWrapper);
        }

        private ImageFileHash? CreateImageHash(string filename)
        {
            ImageFileHash? ret = null;
            try
            {
                using var imageStream = _fileScanner.FileWrapper.OpenRead(filename).StreamInstance;
                var hash = _hashAlgorithm.Hash(imageStream);
                ret = new ImageFileHash(filename, hash, _fileScanner.FileWrapper.GetLastWriteTimeUtc(filename), _fileScanner.FileWrapper);
            }
            catch (SixLabors.ImageSharp.UnknownImageFormatException e)
            {
                _logger.LogExceptionWarning(filename, e);
            }
            catch (Exception e)
            {
                _logger.LogExceptionError(e);
            }

            return ret;
        }
    }
}