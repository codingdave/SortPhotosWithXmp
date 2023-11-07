using MetadataExtractor;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.RearrangeByExif;

internal class RearrangeByExifRunner : IRun
{
    private readonly string _destinationDirectory;
    private readonly string _sourceDirectory;
    private readonly FilesFoundStatistics _statistics;
    private readonly IFileOperation _operationPerformer;
    private readonly IFileScanner _fileScanner;
    private readonly IDirectory _directory;

    private readonly DeleteDirectoryOperation _deleteDirectoryOperation;

    internal RearrangeByExifRunner(ILogger logger,
                             string sourceDirectory,
                             string destinationDirectory,
                             IFileScanner fileScanner,
                             IFile file,
                             IDirectory directory,
                             bool move,
                             bool force)
    {
        _sourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory));
        _destinationDirectory = destinationDirectory ?? throw new ArgumentNullException(nameof(destinationDirectory));
        _operationPerformer = OperationPerformerFactory.GetCopyOrMovePerformer(logger, file, move, force);
        _statistics = new FilesFoundStatistics(logger, _operationPerformer);
        _fileScanner = fileScanner;
        _directory = directory;
        _deleteDirectoryOperation = new DeleteDirectoryOperation(logger, directory, force);
    }

    public IStatistics Run(ILogger logger)
    {
        DateTimeResolver dateTimeResolver = new(logger);
        logger.LogInformation($"Starting {nameof(RearrangeByExifRunner)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'. {_operationPerformer}");

        _fileScanner.Map.Values.AsParallel().ForAll(fileDatum =>
        {
            if (fileDatum.Data != null)
            {
                var file = fileDatum.Data.OriginalFilename;
                try
                {
                    var metaDataDirectories = ImageMetadataReader.ReadMetadata(file);
                    var errors = Helpers.GetErrorsFromMetadata(metaDataDirectories);
                    if (errors.Any())
                    {
                        logger.LogTrace("found errors while extracting metadata from '{file}'", file);
                        _statistics.AddError(new MetaDataError(file, errors));
                    }

                    var possibleDateTime = dateTimeResolver.GetDateTimeFromImage(logger, metaDataDirectories);
                    if (possibleDateTime is DateTime dateTime)
                    {
                        logger.LogTrace("Extracted date {dateTime} from '{file}'", dateTime, file);

                        if (!errors.Any())
                        {
                            MoveImageAndXmpToExifPath(_directory, _fileScanner.Map[file], dateTime, _destinationDirectory, _statistics, _operationPerformer);
                        }
                        else
                        {
                            logger.LogTrace("Keep '{file}' as errors have happened. We will copy it later when dealing about the error.", file);
                        }
                    }
                    else
                    {
                        _statistics.AddError(new NoTimeFoundError(file, Helpers.GetMetadata(metaDataDirectories)));
                    }
                }
                catch (MetadataExtractor.ImageProcessingException e)
                {
                    _statistics.AddError(new ImageProcessingExceptionError(file, e));
                }
                catch (Exception e)
                {
                    logger.LogExceptionError($"Failed processing {file}:", e);
                    _statistics.AddError(new ExceptionError(file, e));
                }
            }
        });

        logger.LogInformation($"{nameof(RearrangeByExifRunner)}.{nameof(Run)} has finished");

        var statistics = new DirectoriesDeletedStatistics(logger, _deleteDirectoryOperation);
        Helpers.RecursivelyDeleteEmptyDirectories(logger, _sourceDirectory, _deleteDirectoryOperation);
        return new FilesAndDirectoriesStatistics(_statistics, statistics);
    }

    private void MoveImageAndXmpToExifPath(
        FileVariations fileVariations,
        DateTime dateTime,
        string destinationDirectory,
        FilesFoundStatistics statistics,
        IFileOperation operationPerformer)
    {
        if (fileVariations.Data == null)
        {
            throw new InvalidOperationException($"The image that shall be moved was not found.");
        }

        if (string.IsNullOrEmpty(destinationDirectory))
        {
            throw new ArgumentException($"'{nameof(destinationDirectory)}' cannot be null or empty.", nameof(destinationDirectory));
        }

        if (statistics is null)
        {
            throw new ArgumentNullException(nameof(statistics));
        }

        if (operationPerformer is null)
        {
            throw new ArgumentNullException(nameof(operationPerformer));
        }

        var destinationSuffix = dateTime.ToString("yyyy/MM/dd");
        var finalDestinationPath = Path.Combine(destinationDirectory, destinationSuffix);

        if (!_directory.Exists(finalDestinationPath))
        {
            _ = _directory.CreateDirectory(finalDestinationPath);
        }

        statistics.FoundImages++;
        statistics.FoundXmps += fileVariations.SidecarFiles.Count;

        foreach (var file in fileVariations.All)
        {
            var targetName = Path.Combine(finalDestinationPath, Path.GetFileName(file.OriginalFilename));

            if (!File.Exists(targetName))
            {
                operationPerformer.ChangeFile(file.OriginalFilename, targetName);
                file.NewFilename = targetName;
            }
            else
            {
                statistics.AddError(new FileAlreadyExistsError(targetName, file.OriginalFilename, $"File {file.OriginalFilename} already exists at {targetName}"));
            }
        }
    }
}