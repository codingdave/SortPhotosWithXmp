namespace SortPhotosWithXmpByExifDate.Repository;

public record struct FileVariationsDto(ImageFileDto? ImageFileDto, List<IImageFileDto> SidecarFileDtos);