namespace SortPhotosWithXmpByExifDate.Repository;

public interface IImageFileDto 
{ 
    string Filename { get; }
    DateTime LastWriteTimeUtc { get; }  
}