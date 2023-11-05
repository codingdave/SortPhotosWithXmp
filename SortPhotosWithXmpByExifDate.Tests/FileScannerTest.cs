using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Moq;

using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

using Xunit;
using Xunit.Sdk;

namespace SortPhotosWithXmpByExifDate.Tests;

public class FileScannerTest
{
    [Fact]
    public void MultipleEditsResolveToSameKey()
    {
        // ifiles
        // DSC_9287.NEF        <-- file, could be mov, jpg, ...
        // DSC_9287.NEF.xmp    <-- 1.st development file, version 0. No versioning for first version.
        // DSC_9287_01.NEF.xmp <-- 2.nd development file, version 1
        // DSC_9287_02.NEF.xmp <-- 3.rd development file, version 2

        // arrange
        var loggerMock = new Mock<ILogger>();
        var fileScanner = new FileScanner(loggerMock.Object);
        var directoryMock = new Mock<IDirectory>();

        var fileList = new List<string>() {
                @"/home/foo/some/path/DSC_9287.NEF",
                @"/home/foo/some/path/DSC_9287.NEF.xmp",
                @"/home/foo/some/path/DSC_9287_01.NEF.xmp",
                @"/home/foo/some/path/DSC_9287_02.NEF.xmp"
                };

        // _ = directoryMock
        //     .Setup(x => x.SetCurrentDirectory(It.IsAny<string>()));

        _ = directoryMock
            .Setup(x => x.GetCurrentDirectory())
            .Returns("some/path");

        _ = directoryMock
            .Setup(x => x.EnumerateFiles(It.IsAny<string>()))
            .Returns(fileList);

        // act
        fileScanner.Crawl(directoryMock.Object);

        
        // var act
        // var result = fileScanner.ExtractFilenameWithoutExtentionAndVersion(filepath);
        // assert
        // Assert.Equal(baseFilename, result);
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
