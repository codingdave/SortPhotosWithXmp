using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class FilesFoundResult : IDirectoryResult, IModifiableErrorCollection
{
    public FilesFoundResult(string directory)
        => (Directory, _errorCollection)
        = (directory, new ErrorCollection.ErrorCollection());

    public IFilesStatistics FilesStatistics { get; } = new FilesStatistics();
    public IReadOnlyErrorCollection ErrorCollection => _errorCollection;
    private readonly IErrorCollection _errorCollection;

    public IReadOnlyPerformerCollection PerformerCollection => _performerCollection;

    public DirectoriesDeletedResult CleanupResult { get; internal set; }

    public string Directory { get; init; }

    private readonly IPerformerCollection _performerCollection = new PerformerCollection();

    public void AddError(IError error)
    {
        _errorCollection.Add(error);
    }

    public void AddPerformer(IPerformer performer)
    {
        _performerCollection.Add(performer);
    }

    public void Log(ILogger logger)
    {
        logger.LogDebug("Logging FilesFoundResult");
        foreach (var error in ErrorCollection.Errors)
        {
            switch (error)
            {
                case FileAlreadyExistsError:
                    // nothing to do over here
                    break;
                case ImageProcessingExceptionError ipe:
                    logger.LogExceptionError(error.File, ipe.Exception);
                    break;
                case GeneralExceptionError:
                case MetaDataError:
                case NoTimeFoundError:
                    logger.LogError(error.ToString());
                    break;
                default:
                    throw new NotImplementedException($"{error}");
            }
        }
    }
}
