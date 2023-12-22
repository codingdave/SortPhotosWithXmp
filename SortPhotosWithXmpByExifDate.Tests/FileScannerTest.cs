using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using Moq;

using SortPhotosWithXmpByExifDate.Features;
using SortPhotosWithXmpByExifDate.Repository;

using SystemInterface.IO;

using Xunit;
using Xunit.Abstractions;

namespace SortPhotosWithXmpByExifDate.Tests;

public class FileScannerTest
{
    private readonly ILogger _logger;
    private readonly FileScanner _fileScanner;
    private readonly Mock<IFile> _fileMock;
    private readonly Mock<IDirectory> _directoryMock;
    private readonly List<string> _images;
    private readonly List<string> _sidecarFiles;
    private readonly List<string> _bogusFiles;
    private readonly ITestOutputHelper _output;

    public FileScannerTest(ITestOutputHelper output)
    {
        _output = output ?? throw new System.ArgumentNullException(nameof(output));

        // arrange
        _logger = _output.BuildLogger();
        _directoryMock = new Mock<IDirectory>(MockBehavior.Strict);
        _ = _directoryMock
            .Setup(x => x.GetCurrentDirectory())
            .Returns("some/path");

        // _ = directoryMock.Setup(x => x.SetCurrentDirectory(It.IsAny<string>()));

        _fileMock = new Mock<IFile>(MockBehavior.Loose);

        _fileScanner = new FileScanner(_logger, _fileMock.Object);

        _images = new List<string>() {
            "/home/foo/some/path/DSC_9287.NEF",
            "some/other/path/050826_foo_03.JPG"
            };

        _sidecarFiles = new List<string>() {
            "/home/foo/some/path/DSC_9287.NEF.xmp",
            "/home/foo/some/path/DSC_9287_01.NEF.xmp",
            "/home/foo/some/path/DSC_9287_02.NEF.xmp",
            "some/other/path/050826_foo_03.JPG.xmp",
            "some/other/path/images/DSC_0051.xmp",
            "some/other/path/images/DSC_0051.01.xmp"
        };

        _bogusFiles = new List<string>() {
            "/home/foo/some/path/DSC_9287_02.NEfF.xmpp"
        };

        var fileList = new List<string>();
        fileList.AddRange(_images);
        fileList.AddRange(_sidecarFiles);
        fileList.AddRange(_bogusFiles);

        _ = _directoryMock
            .Setup(x => x.EnumerateFiles(It.IsAny<string>(), "*", System.IO.SearchOption.AllDirectories))
            .Returns(fileList);
    }

    [Fact]
    public void GetAllImageDataInCurrentDirectory()
    {
        var (images, xmps) = _fileScanner.GetAllImageDataInCurrentDirectory(_directoryMock.Object);
        Assert.Equal(_images, images.ToList());
        Assert.Equal(_sidecarFiles, xmps.ToList());
    }

    // DSC_9287.NEF        <-- file, could be mov, jpg, ...
    // DSC_9287.NEF.xmp    <-- 1.st development file, version 0. No versioning for first version.
    // DSC_9287_01.NEF.xmp <-- 2.nd development file, version 1
    // DSC_9287_02.NEF.xmp <-- 3.rd development file, version 2
    [Fact]
    public void MultipleEditsResolveToSameKey()
    {
        var image0 = GetImageFile("/home/foo/some/path/DSC_9287.NEF");
        var image0Version0 = GetImageFile("/home/foo/some/path/DSC_9287.NEF.xmp");
        var image0Version1 = GetImageFile("/home/foo/some/path/DSC_9287_01.NEF.xmp");
        var image0Version2 = GetImageFile("/home/foo/some/path/DSC_9287_02.NEF.xmp");

        var image1 = GetImageFile("some/other/path/050826_foo_03.JPG");
        var image1Version0 = GetImageFile("some/other/path/050826_foo_03.JPG.xmp");

        var image2Version0 = GetImageFile("some/other/path/images/DSC_0051.xmp");

        var image3Version0 = GetImageFile("some/other/path/images/DSC_0051.01.xmp");

        IEnumerable<FileVariations> fileVariation = new HashSet<FileVariations>()
        {
            // valid multiple edits
            new(image0, new() { image0Version0, image0Version1, image0Version2 }),
            // valid file with edit version 0 that looks like version 3
            new(image1, new() { image1Version0}),
            // no image but xmp found
            new(null, new() { image2Version0}),
            // no image but xmp found
            new(null, new() { image3Version0}),
        };

        // act
        _fileScanner.Crawl(_directoryMock.Object);

        // assert
        Assert.Equal(fileVariation.Count(), _fileScanner.FilenameMap.Values.Count());

        Assert.Equal(fileVariation.ElementAt(0).Data, _fileScanner.FilenameMap.Values.ElementAt(0).Data);
        Assert.Equal(fileVariation.ElementAt(0).SidecarFiles.Count, _fileScanner.FilenameMap.Values.ElementAt(0).SidecarFiles.Count);
        Assert.Equal(fileVariation.ElementAt(0).SidecarFiles.ElementAt(0), _fileScanner.FilenameMap.Values.ElementAt(0).SidecarFiles.ElementAt(0));
        Assert.Equal(fileVariation.ElementAt(0).SidecarFiles.ElementAt(1), _fileScanner.FilenameMap.Values.ElementAt(0).SidecarFiles.ElementAt(1));
        Assert.Equal(fileVariation.ElementAt(0).SidecarFiles.ElementAt(2), _fileScanner.FilenameMap.Values.ElementAt(0).SidecarFiles.ElementAt(2));

        Assert.Equal(fileVariation.ElementAt(1).Data, _fileScanner.FilenameMap.Values.ElementAt(1).Data);
        Assert.Equal(fileVariation.ElementAt(1).SidecarFiles.Count, _fileScanner.FilenameMap.Values.ElementAt(1).SidecarFiles.Count);
        Assert.Equal(fileVariation.ElementAt(1).SidecarFiles.ElementAt(0), _fileScanner.FilenameMap.Values.ElementAt(1).SidecarFiles.ElementAt(0));

        Assert.Equal(fileVariation.ElementAt(2).Data, _fileScanner.FilenameMap.Values.ElementAt(2).Data);
        Assert.Equal(fileVariation.ElementAt(2).SidecarFiles.Count, _fileScanner.FilenameMap.Values.ElementAt(2).SidecarFiles.Count);
        Assert.Equal(fileVariation.ElementAt(2).SidecarFiles.ElementAt(0), _fileScanner.FilenameMap.Values.ElementAt(2).SidecarFiles.ElementAt(0));
    }

    private IImageFile GetImageFile(string filepath)
    {
        return new ImageFile(filepath);
        // var mock = new Mock<IImageFile>(MockBehavior.Strict);
        // _ = mock.Setup(x => x.Filename).Returns(filepath);
        // return mock.Object;
    }

    [Theory]
    // image file
    [InlineData("some/path/DSC_9287.NEF", "some/path/DSC_9287.NEF")]
    // version 0 of the sidecar image file in darktable
    [InlineData("some/path/DSC_9287.NEF", "some/path/DSC_9287.NEF.xmp")]
    // Version 0 will not have the _00 number, so it is a different file
    [InlineData("some/path/DSC_9287_00.NEF", "some/path/DSC_9287_00.NEF.xmp")]
    // version 1 and 2 of the sidecar image file in darktable
    [InlineData("some/path/DSC_9287.NEF", "some/path/DSC_9287_01.NEF.xmp")]
    [InlineData("some/path/DSC_9287.NEF", "some/path/DSC_9287_02.NEF.xmp")]
    // some image file that is similar to the sidecar file structure but not one of those (.xmp missing)
    [InlineData("some/path/DSC_9287_02.NEF", "some/path/DSC_9287_02.NEF")]
    // some/other/path/DSC_0051.01.xmp does not have 2 dots. We keep the filename as is.
    [InlineData("some/other/path/images/DSC_0051.xmp", "some/other/path/images/DSC_0051.xmp")]
    // some/other/path/DSC_0051.xmp does not have an image extension. We keep the filename as is.
    [InlineData("some/other/path/images/DSC_0051.01.xmp", "some/other/path/images/DSC_0051.01.xmp")]

    public void GetBaseFilename(string baseFilename, string filepath)
    {
        // arrange
        // var act
        var result = _fileScanner.ExtractFilenameWithoutExtensionAndVersion(filepath);

        // assert
        Assert.Equal(baseFilename, result);
    }


    [Fact]
    public void GetBaseFilenameWillNotWork()
    {
        // date_action_imageNumber.extension looks like an edit. We can only solve that if we check if an image is called like that.

        // arrange
        var baseFilename = "some/other/path/050826_foo.JPG";
        // presumably version "03" never exists on an image and such is clearly not a version that needs to get stripped off
        var imageFilename = "some/other/path/050826_foo_03.JPG";
        // that, however, cannot be seen by looking at the xmp alone. So the function will "normalize" it, assuming "_03" indicates version 3 and stripping it off
        var xmpFilename = "some/other/path/050826_foo_03.JPG.xmp";

        // var act
        var cleanedXmp = _fileScanner.ExtractFilenameWithoutExtensionAndVersion(xmpFilename);
        var cleanedImage = _fileScanner.ExtractFilenameWithoutExtensionAndVersion(imageFilename);

        // assert
        Assert.NotEqual(baseFilename, cleanedImage);
        Assert.Equal(baseFilename, cleanedXmp);
    }
}
