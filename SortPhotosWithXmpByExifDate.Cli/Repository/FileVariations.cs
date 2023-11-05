namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public record struct FileVariations(IImageFile? Data, List<IImageFile> SidecarFiles);