namespace SortPhotosWithXmpByExifDateCli.Repository;

public record struct FileVariationsDto(ImageFileDto? ImageFileDataDto, List<IImageFileDto> SidecarFileDtos);