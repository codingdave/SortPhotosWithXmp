using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDateCli.Repository;
using SortPhotosWithXmpByExifDateCli.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDateCli.Features.DeleteLonelyXmp;

public class DeleteLeftoverXmpsRunner : IRun
{
    private readonly bool _force;
    private readonly IFileScanner _fileScanner;
    private readonly IFile _fileWrapper;


    public DeleteLeftoverXmpsRunner(bool force, IFileScanner fileScanner, IFile fileWrapper)
    {
        _force = force;
        _fileScanner = fileScanner;
        _fileWrapper = fileWrapper;
    }

    public IStatistics Run(ILogger logger)
    {

        // find all xmps that do not have an image
        var lonelies = _fileScanner.LonelySidecarFiles;
        logger.LogInformation($"Found lonely xmps: {string.Join(", ", lonelies)}");
        if (_force)
        {
            foreach (var lonely in lonelies)
            {
                _fileWrapper.Delete(lonely.Filename);
                throw new NotImplementedException();
            }
        }

        return new DeleteFilesStatistics();
    }
}
