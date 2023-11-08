using SortPhotosWithXmpByExifDate.Cli.ErrorCollection;
using SortPhotosWithXmpByExifDate.Cli.Operations;

namespace SortPhotosWithXmpByExifDate.Cli.Result;

public class FilesAndDirectoriesResult : IResult, IFoundStatistics
{
    private readonly FilesFoundResult _imagesStatistics;
    private readonly DirectoriesDeletedResult _directoriesStatistics;

    public FilesAndDirectoriesResult(
        FilesFoundResult filesStatistics,
        DirectoriesDeletedResult directoriesStatistics)
    {
        (_imagesStatistics, _directoriesStatistics) = (filesStatistics, directoriesStatistics);
        ErrorCollection = new MergedErrorCollection(errorCollection1: _imagesStatistics.ErrorCollection, errorCollection2: _directoriesStatistics.ErrorCollection);
        SuccessfulCollection = new MergedSuccessCollection(_imagesStatistics.SuccessfulCollection, _directoriesStatistics.SuccessfulCollection);
    }

    public IReadOnlyErrorCollection ErrorCollection { get; }
    public IReadOnlySuccessCollection SuccessfulCollection { get; }

    public int FoundXmps { get => _imagesStatistics.FoundXmps; set => _imagesStatistics.FoundXmps = value; }
    public int FoundImages { get => _imagesStatistics.FoundImages; set => _imagesStatistics.FoundImages = value; }
    public int SkippedXmps { get => _imagesStatistics.SkippedXmps; set => _imagesStatistics.SkippedXmps = value; }
    public int SkippedImages { get => _imagesStatistics.SkippedImages; set => _imagesStatistics.SkippedImages = value; }

    public FileOperationBase FileOperation => _imagesStatistics.FileOperation;

    public void Log()
    {
        _imagesStatistics.Log();
        _directoriesStatistics.Log();
    }
}