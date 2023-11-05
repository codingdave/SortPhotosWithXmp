using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Moq;

using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

using Xunit;
using Xunit.Sdk;

namespace SortPhotosWithXmpByExifDate.Tests;

public class FileScannerTest
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly FileScanner _fileScanner;
    private readonly Mock<IDirectory> _directoryMock;
    private readonly List<string> _images;
    private readonly List<string> _sidecarFiles;
    private readonly List<string> _bogusFiles;

    public FileScannerTest()
    {
        // arrange
        _loggerMock = new Mock<ILogger>();
        _fileScanner = new FileScanner(_loggerMock.Object);
        _directoryMock = new Mock<IDirectory>();

        _ = _directoryMock
            .Setup(x => x.GetCurrentDirectory())
            .Returns("some/path");

        // _ = directoryMock.Setup(x => x.SetCurrentDirectory(It.IsAny<string>()));

        _images = new List<string>() {
            @"/home/foo/some/path/DSC_9287.NEF"
            };

        _sidecarFiles = new List<string>() {
            @"/home/foo/some/path/DSC_9287.NEF.xmp",
            @"/home/foo/some/path/DSC_9287_01.NEF.xmp",
            @"/home/foo/some/path/DSC_9287_02.NEF.xmp"
        };

        _bogusFiles = new List<string>() {
            @"/home/foo/some/path/DSC_9287_02.NEfF.xmpp"
        };

        var fileList = new List<string>();
        fileList.AddRange(_images);
        fileList.AddRange(_sidecarFiles);
        fileList.AddRange(_bogusFiles);

        _ = _directoryMock
            .Setup(x => x.EnumerateFiles(It.IsAny<string>()))
            .Returns(fileList);
    }

    [Fact]
    public void GetAllImagesInCurrentDirectory()
    {
        var images = _fileScanner.GetAllImagesInCurrentDirectory(_directoryMock.Object).ToList();
        Assert.Equal(_images, images);
    }

    [Fact]
    public void GetAllXmpsInCurrentDirectory()
    {
        var xmps = _fileScanner.GetAllXmpsInCurrentDirectory(_directoryMock.Object).ToList();
        Assert.Equal(_sidecarFiles, xmps);
    }

    // DSC_9287.NEF        <-- file, could be mov, jpg, ...
    // DSC_9287.NEF.xmp    <-- 1.st development file, version 0. No versioning for first version.
    // DSC_9287_01.NEF.xmp <-- 2.nd development file, version 1
    // DSC_9287_02.NEF.xmp <-- 3.rd development file, version 2
    [Fact]
    public void MultipleEditsResolveToSameKey()
    {
        var regexString = @".*" + FileScanner.XmpExtension + "$";
        var extensionRegex = new Regex(regexString, RegexOptions.IgnoreCase);

        var dir = _directoryMock.Object.GetCurrentDirectory();
        var f = _directoryMock.Object.EnumerateFiles(dir)
        .AsParallel();
        var op = f.Where(x => extensionRegex.IsMatch(x));

        var image = GetIImageFile("/home/foo/some/path/DSC_9287.NEF");
        var imageVersion1 = GetIImageFile("/home/foo/some/path/DSC_9287.NEF.xmp");
        var imageVersion2 = GetIImageFile("/home/foo/some/path/DSC_9287_01.NEF.xmp");
        var imageVersion3 = GetIImageFile("/home/foo/some/path/DSC_9287_02.NEF.xmp");

        IEnumerable<FileVariations> fileVariation = new HashSet<FileVariations>()
        {
            new(image, new() { imageVersion1, imageVersion2, imageVersion3 })
        };

        // act
        _fileScanner.Crawl(_directoryMock.Object);

        // assert
        Assert.Equal(fileVariation.Count(), _fileScanner.All.Count());
        Assert.Equal(fileVariation.First().Data, _fileScanner.All.First().Data);
        Assert.Equal(fileVariation.First().SidecarFiles.Count, _fileScanner.All.First().SidecarFiles.Count);
        Assert.Equal(fileVariation.First().SidecarFiles.ElementAt(0), _fileScanner.All.First().SidecarFiles.ElementAt(0));
        Assert.Equal(fileVariation.First().SidecarFiles.ElementAt(1), _fileScanner.All.First().SidecarFiles.ElementAt(1));
        Assert.Equal(fileVariation.First().SidecarFiles.ElementAt(2), _fileScanner.All.First().SidecarFiles.ElementAt(2));
    }

    private IImageFile GetIImageFile(string filepath)
    {
        return new ImageFile(filepath);
        // var mock = new Mock<IImageFile>();
        // _ = mock.Setup(x => x.Filename).Returns(filepath);
        // return mock.Object;
    }

    [Fact]
    public void Test1()
    {
        // Multiple edits share the same originating file/key DSC_9287.NEF:
        // DSC_9287.NEF        <-- file, could be mov, jpg, ...
        // DSC_9287.NEF.xmp    <-- 1.st development file, version 0. No versioning for first version.
        // DSC_9287_01.NEF.xmp <-- 2.nd development file, version 1
        // DSC_9287_02.NEF.xmp <-- 3.rd development file, version 2

        // Multiple filetypes make the extension mandatory: 
        // DSC_7708.JPG
        // DSC_7708.JPG.xmp
        // DSC_7708.NEF
        // DSC_7708.NEF.xmp

        // ---

        // images/20170617/
        // DSC_3398.NEF
        // DSC_3398.NEF.xmp
        // DSC_3398_01.NEF.xmp
        // FN.EXT
        // FN(_\d\d).EXT.xmp

        // IMG_20160417_122849_1.JPG.xmp

        // [01:18:56 ERR] The file 
        // '~/projects/SortPhotosWithXmpByExifDate/resources/Photos-copy/Fotos/
        // 20160417/
        // IMG_20160417_122849_1.jpg' 
        // has an invalid name: Sidecar files will not be distiguishable from 
        // edits of another file. The convention to name them is: 
        // filename_number.extension.xmp, which matches this filename.
        // "deleteLeftoverXmps",
        // "--source",
        // "~/projects/SortPhotosWithXmpByExifDate/resources/Photos-copy",

        // // the filename can not get differentiated to an edit revision
        // var filepath = "~/projects/SortPhotosWithXmpByExifDate/resources/Photos-copy/Fotos/20160417/IMG_20160417_122849_1.jpg";
        // var loggerMock = new Mock<ILogger>();
        // var fileScanner = new FileScanner(loggerMock.Object);
        // Assert.Matches(fileScanner.ImageFileWithRevision, filepath);

        // var imageFile = new ImageFile(filepath);
        // var fileVariation = new FileVariations();
        // var fileMock = new Mock<IFile>();
        // var fileScannerMock = new Mock<IFileScanner>();
        // _ = fileScannerMock.Setup(x => x.All).Returns(new List<FileVariations>() { fileVariation });
        // // fileMock.Setup(x => x.)
        // var force = true;
        // var deleteLeftoverXmps = new DeleteLeftoverXmpsRunner(
        //     force,
        //     fileScannerMock.Object,
        //     fileMock.Object);
        // var statistics = deleteLeftoverXmps.Run(loggerMock.Object);

        // fileScannerMock.Object.NotSupportedNamingReg


        // FileScanner GenerateDatabase
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
    public void GetBaseFilename(string baseFilename, string filepath)
    {
        // arrange
        var loggerMock = new Mock<ILogger>();
        var fileScanner = new FileScanner(loggerMock.Object);

        // var act
        var result = fileScanner.ExtractFilenameWithoutExtentionAndVersion(filepath);

        // assert
        Assert.Equal(baseFilename, result);
    }
}
