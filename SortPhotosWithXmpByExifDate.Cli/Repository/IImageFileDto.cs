namespace SortPhotosWithXmpByExifDate.Cli.Repository;

public interface IImageFileDto 
{ 
    string Filename { get; }
    DateTime LastWriteTimeUtc { get; }  
}