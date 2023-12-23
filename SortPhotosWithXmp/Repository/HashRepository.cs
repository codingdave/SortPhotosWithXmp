using System.Text.Json;

using AutoMapper;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Extensions;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Repository;

internal class HashRepository
{
    private readonly ILogger _logger;
    private readonly string _baseDirectory;
    private readonly IFile _file;
    private readonly Mapper _mapper = AutoMapperConfiguration.InitializeAutomapper();
    private readonly string _filename;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly object _lock = new();

    public HashRepository(ILogger logger, string baseDirectory, IFile file)
    {
        if (string.IsNullOrEmpty(baseDirectory))
        {
            throw new ArgumentException($"'{nameof(baseDirectory)}' cannot be null or empty.", nameof(baseDirectory));
        }

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _baseDirectory = baseDirectory;
        _file = file ?? throw new ArgumentNullException(nameof(file));
        _filename = $"{_baseDirectory}fileData.json";

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    internal HashSet<FileVariations> ReadRepository()
    {
        _logger.LogWarning($"calling {nameof(ReadRepository)}");
        lock (_lock)
        {
            HashSet<FileVariations> fileData = new();

            if (File.Exists(_filename))
            {
                try
                {
#warning Check invalidation rules for 1. filename different/null, 2. any sidecar file
                    _logger.LogInformation($"Loading file data from a previous run from '{_filename}'.");
                    var fileDataDto = JsonSerializer.Deserialize<IEnumerable<FileVariationsDto>>(_file.ReadAllText(_filename))!;
                    fileData = fileDataDto.Select(x => _mapper.Map<FileVariations>(x))
                    .Where(x => x.Data != null
                                && _file.Exists(x.Data.CurrentFilename)
                                && x.Data.LastWriteTimeUtc == _file.GetLastWriteTimeUtc(x.Data.CurrentFilename)).ToHashSet();
                }
                catch (Exception e)
                {
                    _logger.LogExceptionError(e);
                }
            }
            else
            {
                _logger.LogInformation("No image data can get loaded from a previous run.");
            }

            return fileData;
        }
    }

    internal void SaveRepository(IEnumerable<FileVariations> fileVariations)
    {
        lock (_lock)
        {
            _logger.LogWarning($"calling {nameof(SaveRepository)}");

            var fileVariationDtos = fileVariations.Select(x => _mapper.Map<FileVariationsDto>(x)).ToList();
            if (fileVariationDtos.Any())
            {
                File.WriteAllText(_filename, JsonSerializer.Serialize(fileVariationDtos, _jsonSerializerOptions));
            }
        }
    }
}