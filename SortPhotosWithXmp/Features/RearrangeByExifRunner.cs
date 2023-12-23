using MetadataExtractor;

using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.ErrorHandlers;
using SortPhotosWithXmp.Extensions;
using SortPhotosWithXmp.Operation;
using SortPhotosWithXmp.Result;

using SystemInterface.IO;
using MetadataExtractor.Formats.Xmp;
using SortPhotosWithXmp.Performer;

namespace SortPhotosWithXmp.Features;

public class RearrangeByExifRunner : IRun
{
    private readonly string _destinationDirectory;
    private readonly string _sourceDirectory;
    private readonly FilesFoundResult _filesFoundResult;
    private readonly FileOperationBase _fileOperation;
    private readonly DeleteFileOperation _deleteOperation;
    private readonly IFileScanner _fileScanner;
    private readonly IDirectory _directoryWrapper;

    public RearrangeByExifRunner(ILogger logger,
                             string sourceDirectory,
                             string destinationDirectory,
                             IFileScanner fileScanner,
                             IFile fileWrapper,
                             IDirectory directoryWrapper,
                             bool isMove,
                             bool isForce)
    {
        _sourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory));
        _destinationDirectory = destinationDirectory ?? throw new ArgumentNullException(nameof(destinationDirectory));
        _deleteOperation = new DeleteFileOperation(logger, fileWrapper, directoryWrapper, isForce);
        _filesFoundResult = new FilesFoundResult(logger, fileWrapper, directoryWrapper, destinationDirectory, isForce, new DeleteDirectoriesPerformer(_sourceDirectory, _deleteOperation));
        var errorhandler = (FileAlreadyExistsError e) => _filesFoundResult.FileAlreadyExistsErrorPerformer.Errors.Add(e);
        _fileOperation = OperationFactory.GetCopyOrMoveOperation(logger, fileWrapper, directoryWrapper, errorhandler, isMove, isForce);
        _fileScanner = fileScanner;
        _directoryWrapper = directoryWrapper;
    }

    public bool IsForce => _fileOperation.IsForce;

    public IResult Run(ILogger logger)
    {
        DateTimeResolver dateTimeResolver = new(logger);
        logger.LogInformation($"Starting {nameof(RearrangeByExifRunner)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'. {_fileOperation}");
        
        _fileScanner.FilenameMap.Values
#if RELEASE
        .AsParallel().ForAll(
#else
        .Do(
#endif
        fileDatum =>
        {
            if (fileDatum.Data != null)
            {
                var file = fileDatum.Data.OriginalFilename;
                try
                {
                    var metaDataDirectories = ImageMetadataReader.ReadMetadata(file);

                    var possibleDateTime = dateTimeResolver.GetDateTimeFromImage(logger, metaDataDirectories);
                    if (possibleDateTime is DateTime dateTime)
                    {
                        // when we can extract a date, there is no error for our usecase
                        _filesFoundResult.Performers.Add(new ToExifPathPerformer(fileDatum, _destinationDirectory, dateTime, _fileOperation));
                        _filesFoundResult.FilesStatistics.FoundImages++;
                        _filesFoundResult.FilesStatistics.FoundXmps += fileDatum.SidecarFiles.Count;
                    }
                    else
                    {
                        // error: we need the date for rearranging
                        _filesFoundResult.NoTimeFoundErrorPerformer.Errors.Add(new NoTimeFoundError(file, GetMetadata(metaDataDirectories)));
                    }

                    var metaDataErrors = metaDataDirectories.SelectMany(t => t.Errors);
                    if (metaDataErrors.Any())
                    {
                        logger.LogTrace("found errors in the metadata while extracting metadata from '{file}': {errors}", file, string.Join(Environment.NewLine, metaDataErrors));
                        _filesFoundResult.MetaDataErrorPerformer.Errors.Add(new MetaDataError(file, metaDataErrors));
                    }
                }
                catch (MetadataExtractor.ImageProcessingException e)
                {
                    _filesFoundResult.ImageProcessingExceptionErrors.Add(new ImageProcessingExceptionError(file, e));
                }
                catch (Exception e)
                {
                    _filesFoundResult.GeneralExceptionErrors.Add(new GeneralExceptionError(file, e));
                }
            }
        });

        logger.LogInformation($"{nameof(RearrangeByExifRunner)}.{nameof(Run)} has finished");

        return _filesFoundResult;
    }


    private static List<string> GetMetadata(IReadOnlyList<MetadataExtractor.Directory> metaDataDirectories)
    {
        var ret = new List<string>();
        foreach (var metadataDirectory in metaDataDirectories)
        {
            foreach (var tag in metadataDirectory.Tags)
            {
                ret.Add($"{metadataDirectory.Name} - {tag.Name} = {tag.Description}");
            }
            if (metadataDirectory is XmpDirectory xmpDirectory)
            {
                ret.AddRange(GetPropertyDescriptions(xmpDirectory));
            }
        }

        return ret;
    }

    private static IList<string> GetPropertyDescriptions(XmpDirectory xmpDirectory)
    {
        List<string> propertyDescriptions = new();
        if (xmpDirectory.XmpMeta != null)
        {
            foreach (var property in xmpDirectory.XmpMeta.Properties)
            {
                if (property.Path != null && property.Value != null)
                {
                    propertyDescriptions.Add($"{property.Path}: {property.Value}");
                }
            }
        }

        return propertyDescriptions;
    }
}