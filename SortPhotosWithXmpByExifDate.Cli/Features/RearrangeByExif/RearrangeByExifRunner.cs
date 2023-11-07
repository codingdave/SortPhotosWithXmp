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
    private readonly FilesFoundStatistics _filesFoundStatistics;
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
        _operationPerformer = OperationPerformerFactory.GetCopyOrMovePerformer(logger, file, directory, move, force);
        _filesFoundStatistics = new FilesFoundStatistics(logger, _operationPerformer);
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
                        _filesFoundStatistics.AddError(new MetaDataError(file, errors));
                    }

                    var possibleDateTime = dateTimeResolver.GetDateTimeFromImage(logger, metaDataDirectories);
                    if (possibleDateTime is DateTime dateTime)
                    {
                        logger.LogTrace("Extracted date {dateTime} from '{file}'", dateTime, file);

                        if (!errors.Any())
                        {
                            MoveImageAndXmpToExifPath(_fileScanner.Map[file], dateTime);
                        }
                        else
                        {
                            logger.LogTrace("Keep '{file}' as errors have happened. We will copy it later when dealing about the error.", file);
                        }
                    }
                    else
                    {
                        _filesFoundStatistics.AddError(new NoTimeFoundError(file, Helpers.GetMetadata(metaDataDirectories)));
                    }
                }
                catch (MetadataExtractor.ImageProcessingException e)
                {
                    _filesFoundStatistics.AddError(new ImageProcessingExceptionError(file, e));
                }
                catch (Exception e)
                {
                    logger.LogExceptionError($"Failed processing {file}:", e);
                    _filesFoundStatistics.AddError(new GeneralExceptionError(file, e));
                }
            }
        });

        logger.LogInformation($"{nameof(RearrangeByExifRunner)}.{nameof(Run)} has finished");

        var directoriesDeletedStatistics = new DirectoriesDeletedStatistics(logger, _deleteDirectoryOperation);
        Helpers.RecursivelyDeleteEmptyDirectories(logger, _sourceDirectory, _deleteDirectoryOperation);
        return new FilesAndDirectoriesStatistics(_filesFoundStatistics, directoriesDeletedStatistics);
    }

    private void MoveImageAndXmpToExifPath(FileVariations fileVariations, DateTime dateTime)
    {
        if (fileVariations.Data == null)
        {
            throw new InvalidOperationException($"The image that shall be moved was not found.");
        }

        var destinationSuffix = dateTime.ToString("yyyy/MM/dd");
        var targetPath = Path.Combine(_destinationDirectory, destinationSuffix);

        _filesFoundStatistics.FoundImages++;
        _filesFoundStatistics.FoundXmps += fileVariations.SidecarFiles.Count;
        _operationPerformer.ChangeFiles(fileVariations.All, targetPath);
    }
}