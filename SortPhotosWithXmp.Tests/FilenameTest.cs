using SortPhotosWithXmp.Repository;

using SystemWrapper.IO;

using Xunit;

namespace SortPhotosWithXmp.Tests;

public class FilenameTests
{
    [Fact]
    public void GetAllImagesInCurrentDirectory()
    {
        var oldName = "foo/old.name";
        var newName = "bar/new.name";
        var fileWraper = new FileWrap();
        var image = new ImageFile(oldName, fileWraper);
        Assert.Equal(oldName, image.OriginalFilename);
        Assert.Equal(oldName, image.CurrentFilename);
        Assert.Null(image.NewFilename);

        image.NewFilename = newName;
        Assert.Equal(oldName, image.OriginalFilename);
        Assert.Equal(newName, image.CurrentFilename);
        Assert.Equal(newName, image.NewFilename);
    }
}