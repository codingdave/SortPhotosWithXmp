namespace SortPhotosWithXmpByExifDateCli.Repository;

public record struct FileVariations(IImageFile? Data, List<IImageFile> SidecarFiles);