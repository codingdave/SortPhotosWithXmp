using AutoMapper;

namespace SortPhotosWithXmpByExifDateCli.Repository;

public class AutoMapperConfiguration
{
    public static Mapper InitializeAutomapper()
    {
        var config = new MapperConfiguration(mapperConfigurationExpression =>
        {
            mapperConfigurationExpression.CreateMap<ImageFile, ImageFileDto>();
            mapperConfigurationExpression.CreateMap<ImageFileDto, ImageFile>();

            mapperConfigurationExpression.CreateMap<ImageFileHash, ImageFileHashDto>();
            mapperConfigurationExpression.CreateMap<ImageFileHashDto, ImageFileHash>();

            mapperConfigurationExpression.CreateMap<SidecarFileHash, SidecarFileHashDto>();
            mapperConfigurationExpression.CreateMap<SidecarFileHashDto, SidecarFileHash>();

            mapperConfigurationExpression.CreateMap<FileVariations, FileVariationsDto>();
            mapperConfigurationExpression.CreateMap<FileVariationsDto, FileVariations>();
        });
        return new Mapper(config);
    }
}