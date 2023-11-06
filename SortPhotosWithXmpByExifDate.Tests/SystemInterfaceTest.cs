using System.Collections.Generic;

using Moq;

using SystemInterface.IO;

using SystemWrapper.IO;

using Xunit;

namespace SortPhotosWithXmpByExifDate.Tests;

public class SystemInterfaceTest
{
    [Fact]
    public void TestMocking()
    {
        var directoryMock = new Mock<IDirectory>();
        var path = @"/home/david/projects/SortPhotosWithXmpByExifDate.Cli";
        var fileList = new List<string>() { @"c:\test.txt", @"c:\test2.txt" };
        _ = directoryMock
            .Setup(x => x.GetCurrentDirectory())
            .Returns(path);

        _ = directoryMock
            .Setup(x => x.SetCurrentDirectory(It.IsAny<string>()));

        _ = directoryMock
            .Setup(x => x.EnumerateFiles(It.IsAny<string>()))
            .Returns(new[] { @"c:\test.txt", @"c:\test2.txt" });

        Assert.Equal(path, directoryMock.Object.GetCurrentDirectory());
        Assert.Equal(fileList, directoryMock.Object.EnumerateFiles(@"path/does/not/matter"));
    }

    [Fact]
    public void CheckWrapWorks()
    {
        // Arrange
        var realDirectory = new DirectoryWrap();

        var path = "/home";

        // Act
        realDirectory.SetCurrentDirectory(path);

        // Assert
        Assert.Equal(path, realDirectory.GetCurrentDirectory());
    }
}
