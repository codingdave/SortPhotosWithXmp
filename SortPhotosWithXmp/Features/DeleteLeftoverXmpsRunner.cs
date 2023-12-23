using Microsoft.Extensions.Logging;

using SortPhotosWithXmp.Result;

using SystemInterface.IO;

namespace SortPhotosWithXmp.Features;

public class DeleteLeftoverXmpsRunner : IRun
{
    public bool IsForce { get; }
    private readonly IFileScanner _fileScanner;
    private readonly IFile _fileWrapper;

    public DeleteLeftoverXmpsRunner(bool isForce, IFileScanner fileScanner, IFile fileWrapper)
    {
        IsForce = isForce;
        _fileScanner = fileScanner;
        _fileWrapper = fileWrapper;
    }

    public IResult Run(ILogger logger)
    {
        // find all xmps that do not have an image
        var lonelies = _fileScanner.LonelySidecarFiles;
        logger.LogInformation($"Found lonely xmps: {string.Join(", ", lonelies)}");
        if (IsForce)
        {
            foreach (var lonely in lonelies)
            {
                _fileWrapper.Delete(lonely.CurrentFilename);
                throw new NotImplementedException();
            }
        }

        return new DeleteFilesResult();
    }
}
