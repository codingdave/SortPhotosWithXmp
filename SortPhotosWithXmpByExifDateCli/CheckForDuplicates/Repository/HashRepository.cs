using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;

namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates.Store
{
    internal class HashRepository
    {
        private readonly ILogger _logger;
        private readonly string _baseDirectory;
        private readonly Mapper _mapper = AutoMapperConfiguration.InitializeAutomapper();
        private readonly string _xmpHashesFilename;
        private readonly string _imageHashesFilename;
        object _lock = new();
        public HashRepository(ILogger logger, string baseDirectory)
        {
            _logger = logger;
            _baseDirectory = baseDirectory;
            _xmpHashesFilename = $"{_baseDirectory}xmpHashes.json";
            _imageHashesFilename = $"{_baseDirectory}imageHashes.json";
        }

        internal (List<XmpHash> xmpHashes, List<ImageHash> imageHashes) ReadHashes()
        {
            lock (_lock)
            {
                List<XmpHash> xmpHashes = new();
                List<ImageHash> imageHashes = new();
                _logger.LogWarning($"calling {nameof(ReadHashes)}");

                if (File.Exists(_xmpHashesFilename))
                {
                    try
                    {
                        _logger.LogInformation("Loading xmp hashes from a previous run.");
                        var xmpDtoHashes = JsonSerializer.Deserialize<IEnumerable<XmpHashDto>>(File.ReadAllText(_xmpHashesFilename))!;
                        xmpHashes = xmpDtoHashes.Select(x => _mapper.Map<XmpHash>(x))
                        .Where(x => x.LastWriteTimeUtc == File.GetLastWriteTimeUtc(x.Filename))
                        .ToList();
                    }
                    catch (Exception e)
                    {
                        _logger.LogExceptionError(e);
                    }
                }
                else
                {
                    _logger.LogInformation("No xmp hashes can get loaded from a previous run.");
                }

                if (File.Exists(_imageHashesFilename))
                {
                    try
                    {
                        _logger.LogInformation("Loading image hashes from a previous run.");
                        var imageDtoHashes = JsonSerializer.Deserialize<IEnumerable<ImageHashDto>>(File.ReadAllText(_imageHashesFilename))!;
                        imageHashes = imageDtoHashes.Select(x => _mapper.Map<ImageHash>(x))
                        .Where(x => x.LastWriteTimeUtc == File.GetLastWriteTimeUtc(x.Filename))
                        .ToList();
                    }
                    catch (Exception e)
                    {
                        _logger.LogExceptionError(e);
                    }
                }
                else
                {
                    _logger.LogInformation("No image hashes can get loaded from a previous run.");
                }

                return (xmpHashes, imageHashes);
            }
        }

        internal object? SaveHashes(List<XmpHash> xmpHashes, List<ImageHash> imageHashes)
        {
            lock (_lock)
            {
                _logger.LogWarning($"calling {nameof(SaveHashes)}");
                var xmpDtoHashes = xmpHashes.Select(x => _mapper.Map<XmpHashDto>(x)).ToList();
                var imageDtoHashes = imageHashes.Select(x => _mapper.Map<ImageHashDto>(x)).ToList();

                File.WriteAllText(_xmpHashesFilename, JsonSerializer.Serialize(xmpDtoHashes));
                File.WriteAllText(_imageHashesFilename, JsonSerializer.Serialize(imageDtoHashes));

                return null;
            }
        }
    }
}