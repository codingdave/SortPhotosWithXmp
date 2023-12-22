namespace SortPhotosWithXmp.Repository;

public record struct FileVariationsDto(ImageFileDto? ImageFileDto, List<IImageFileDto> SidecarFileDtos);