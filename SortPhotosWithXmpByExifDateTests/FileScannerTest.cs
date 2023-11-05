using Microsoft.Extensions.Logging;
using Moq;
using SortPhotosWithXmpByExifDateCli.Repository;
using Xunit;

namespace SortPhotosWithXmpByExifDateTests;

public partial class FileScannerTest
{
    [Fact]
    public void Test1()
    {
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

        // the filename can not get differentiated to an edit revision
        var filepath = "~/projects/SortPhotosWithXmpByExifDate/resources/Photos-copy/Fotos/20160417/IMG_20160417_122849_1.jpg";
        var loggerMock = new Mock<ILogger>();
        var fileScanner = new FileScanner(loggerMock.Object);
        Assert.Matches(fileScanner.ImageFileWithRevision, filepath);

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
}
