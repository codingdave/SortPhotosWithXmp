using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Operation;
using SortPhotosWithXmpByExifDate.Performer;
using SortPhotosWithXmpByExifDate.Repository;

namespace SortPhotosWithXmpByExifDate.Features.RearrangeByExif;

internal class ToExifPathPerformer : IPerformer
{
    private readonly FileVariations _file;
    private readonly string _destinationDirectory;
    private readonly DateTime _dateTime;
    private readonly FileOperationBase _operation;

    public ToExifPathPerformer(FileVariations file, string destinationDirectory, DateTime dateTime, FileOperationBase operation)
    {
        if (file.Data == null)
        {
            throw new InvalidOperationException($"The addressed image was not found.");
        }

        _file = file;
        _destinationDirectory = destinationDirectory;
        _dateTime = dateTime;
        _operation = operation;
    }

    public void Perform(ILogger logger)
    {
        logger.LogTrace("Extracted date {dateTime} from '{file}'", _dateTime, _file);

        var destinationSuffix = _dateTime.ToString("yyyy/MM/dd");
        var targetPath = _operation.JoinDirectory(_destinationDirectory, destinationSuffix);
        _operation.ChangeFiles(_file.All, targetPath);
    }
}