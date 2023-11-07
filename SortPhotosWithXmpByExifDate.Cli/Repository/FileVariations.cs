namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public record struct FileVariations(IImageFile? Data, List<IImageFile> SidecarFiles)
{
    public IEnumerable<IImageFile> All
    {
        get
        {
            var all = new List<IImageFile>() { };
            if (Data != null)
            {
                all.Add(Data);
            }
            all.AddRange(SidecarFiles);
            return all;
        }
    }
}