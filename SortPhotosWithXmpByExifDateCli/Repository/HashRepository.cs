using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SortPhotosWithXmpByExifDateCli.CheckForDuplicates;
using SortPhotosWithXmpByExifDateCli.Entities;
using SortPhotosWithXmpByExifDateCli.ErrorCollection;

namespace SortPhotosWithXmpByExifDateCli.Repository;

internal class HashRepository
{
    private readonly ILogger _logger;
    private readonly string _baseDirectory;
    private readonly Mapper _mapper = AutoMapperConfiguration.InitializeAutomapper();
    private readonly string _xmpHashesFilename;
    private readonly string _imageHashesFilename;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly object _lock = new();

    public HashRepository(ILogger logger, string baseDirectory)
    {
        _logger = logger;
        _baseDirectory = baseDirectory;
        _xmpHashesFilename = $"{_baseDirectory}xmpHashes.json";
        _imageHashesFilename = $"{_baseDirectory}imageHashes.json";

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    internal (Dictionary<string, XmpHash> xmpHashes, Dictionary<string, ImageHash> imageHashes) ReadHashes()
    {
        lock (_lock)
        {
            Dictionary<string, XmpHash> xmpHashes = new();
            Dictionary<string, ImageHash> imageHashes = new();
            _logger.LogWarning($"calling {nameof(ReadHashes)}");

            if (File.Exists(_xmpHashesFilename))
            {
                try
                {
                    _logger.LogInformation($"Loading xmp hashes from a previous run from '{_xmpHashesFilename}'.");
                    var xmpDtoHashes = JsonSerializer.Deserialize<IEnumerable<XmpHashDto>>(File.ReadAllText(_xmpHashesFilename))!;
                    xmpHashes = xmpDtoHashes.Select(x => _mapper.Map<XmpHash>(x))
                    .Where(x => File.Exists(x.Filename))
                    .Where(x => x.LastWriteTimeUtc == File.GetLastWriteTimeUtc(x.Filename))
                    .ToDictionary(x => x.Filename, x => x);
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
                    _logger.LogInformation($"Loading image hashes from a previous run from '{_imageHashesFilename}'.");
                    var text = File.ReadAllText(_imageHashesFilename);
                    var imageDtoHashes = JsonSerializer.Deserialize<IEnumerable<ImageHashDto>>(text)!;
                    imageHashes = imageDtoHashes.Select(x => _mapper.Map<ImageHash>(x))
                    .Where(x => File.Exists(x.Filename))
                    .Where(x => x.LastWriteTimeUtc == File.GetLastWriteTimeUtc(x.Filename))
                    .ToDictionary(x => x.Filename, x => x);
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

    internal void SaveHashes(IEnumerable<XmpHash> xmpHashes, IEnumerable<ImageHash> imageHashes)
    {
        lock (_lock)
        {
            _logger.LogWarning($"calling {nameof(SaveHashes)}");

            var xmpDtoHashes = xmpHashes.Select(x => _mapper.Map<XmpHashDto>(x)).ToList();
            if (xmpDtoHashes.Any())
            {
                File.WriteAllText(_xmpHashesFilename, JsonSerializer.Serialize(xmpDtoHashes, _jsonSerializerOptions));
            }

            var imageDtoHashes = imageHashes.Select(x => _mapper.Map<ImageHashDto>(x)).ToList();
            if (imageDtoHashes.Any())
            {
                File.WriteAllText(_imageHashesFilename, JsonSerializer.Serialize(imageDtoHashes, _jsonSerializerOptions));
            }
        }
    }
}