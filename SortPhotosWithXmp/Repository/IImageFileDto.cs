namespace SortPhotosWithXmp.Repository;

public interface IImageFileDto 
{ 
    string Filename { get; }
    DateTime LastWriteTimeUtc { get; }  
}