using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Statistics;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.DeleteLonelyXmp;

public class DeleteLeftoverXmpsRunner : IRun
{
    private readonly bool _force;
    private readonly IFileScanner _fileScanner;
    private readonly IFile _file;


    public DeleteLeftoverXmpsRunner(bool force, IFileScanner fileScanner, IFile file)
    {
        _force = force;
        _fileScanner = fileScanner;
        _file = file;
    }

    public IResult Run(ILogger logger)
    {

        // find all xmps that do not have an image
        var lonelies = _fileScanner.LonelySidecarFiles;
        logger.LogInformation($"Found lonely xmps: {string.Join(", ", lonelies)}");
        if (_force)
        {
            foreach (var lonely in lonelies)
            {
                _file.Delete(lonely.OriginalFilename);
                throw new NotImplementedException();
            }
        }

        return new DeleteFilesResult();
    }
}
