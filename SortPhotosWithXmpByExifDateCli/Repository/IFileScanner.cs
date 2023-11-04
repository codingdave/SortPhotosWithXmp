namespace SortPhotosWithXmpByExifDateCli.Repository;

public interface IFileScanner
{
    public static string SidecarFileExtension { get; } = ".xmp";
    public IEnumerable<FileVariations> All { get; }

    public IEnumerable<FileVariations> MultipleEdits { get; }
    // having sidecar files but no source data is not healthy
    public IEnumerable<IImageFile> LonelySidecarFiles { get; }
    // we have an source, not necessary any sidecar files.
    public IEnumerable<FileVariations> HealtyFileVariations { get; }
}