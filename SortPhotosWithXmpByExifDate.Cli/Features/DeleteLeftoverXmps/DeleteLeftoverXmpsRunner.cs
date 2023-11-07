using Microsoft.Extensions.Logging;

using SortPhotosWithXmpByExifDate.Cli.Repository;
using SortPhotosWithXmpByExifDate.Cli.Result;

using SystemInterface.IO;

namespace SortPhotosWithXmpByExifDate.Cli.Features.DeleteLonelyXmp;

public class DeleteLeftoverXmpsRunner : IRun
{
    public bool Force {get;set;}
    private readonly IFileScanner _fileScanner;
    private readonly IFile _file;

    public DeleteLeftoverXmpsRunner(bool force, IFileScanner fileScanner, IFile file)
    {
        Force = force;
        _fileScanner = fileScanner;
        _file = file;
    }

    public IResult Run(ILogger logger)
    {
        // find all xmps that do not have an image
        var lonelies = _fileScanner.LonelySidecarFiles;
        logger.LogInformation($"Found lonely xmps: {string.Join(", ", lonelies)}");
        if (Force)
        {
            foreach (var lonely in lonelies)
            {
                _file.Delete(lonely.CurrentFilename);
                throw new NotImplementedException();
            }
        }

        return new DeleteFilesResult();
    }
}
