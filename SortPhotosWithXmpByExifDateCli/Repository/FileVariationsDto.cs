namespace SortPhotosWithXmpByExifDateCli.Repository;

public record struct FileVariationsDto(ImageFileDto? ImageFileDto, List<IImageFileDto> SidecarFileDtos);