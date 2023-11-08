using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Operations;
using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.Features.RearrangeByExif;

internal class ToExifPath : ISuccess
{
    private readonly FileVariations _file;
    private readonly string _destinationDirectory;
    private readonly DateTime _dateTime;
    private readonly FileOperationBase _operationPerformer;

    public ToExifPath(FileVariations file, string destinationDirectory, DateTime dateTime, FileOperationBase operationPerformer)
    {
        _file = file;
        _destinationDirectory = destinationDirectory;
        _dateTime = dateTime;
        _operationPerformer = operationPerformer;

        if (_file.Data == null)
        {
            throw new InvalidOperationException($"The addressed image was not found.");
        }
    }

    public void Perform(ILogger logger)
    {
        logger.LogTrace("Extracted date {dateTime} from '{file}'", _dateTime, _file);

        var destinationSuffix = _dateTime.ToString("yyyy/MM/dd");
        var targetPath = Path.Combine(_destinationDirectory, destinationSuffix);
        _operationPerformer.ChangeFiles(_file.All, targetPath);
    }
}