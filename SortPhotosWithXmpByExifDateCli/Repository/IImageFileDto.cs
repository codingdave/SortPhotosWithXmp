namespace SortPhotosWithXmpByExifDateCli.Repository;

public interface IImageFileDto 
{ 
    string Filename { get; }
    DateTime LastWriteTimeUtc { get; }  
}