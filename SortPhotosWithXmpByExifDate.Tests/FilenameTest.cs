using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Moq;

using SixLabors.ImageSharp;

using SortPhotosWithXmpByExifDate.Cli.Repository;

using SystemInterface.IO;

using Xunit;

namespace SortPhotosWithXmpByExifDate.Tests;

public class FilenameTests
{
    public FilenameTests()
    {

    }

    [Fact]
    public void GetAllImagesInCurrentDirectory()
    {
        var oldName = "foo/old.name";
        var newName = "bar/new.name";

        var image = new ImageFile(oldName);
        Assert.Equal(oldName, image.OriginalFilename);
        Assert.Equal(oldName, image.CurrentFilename);
        Assert.Null(image.NewFilename);

        image.NewFilename = newName;
        Assert.Equal(oldName, image.OriginalFilename);
        Assert.Equal(newName, image.CurrentFilename);
        Assert.Equal(newName, image.NewFilename);
    }
}