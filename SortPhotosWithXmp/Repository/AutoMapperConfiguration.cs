using AutoMapper;

namespace SortPhotosWithXmp.Repository;

public class AutoMapperConfiguration
{
    public static Mapper InitializeAutomapper()
    {
        var config = new MapperConfiguration(mapperConfigurationExpression =>
        {
            _ = mapperConfigurationExpression.CreateMap<ImageFile, ImageFileDto>();
            _ = mapperConfigurationExpression.CreateMap<ImageFileDto, ImageFile>();

            _ = mapperConfigurationExpression.CreateMap<ImageFileHash, ImageFileHashDto>();
            _ = mapperConfigurationExpression.CreateMap<ImageFileHashDto, ImageFileHash>();

            _ = mapperConfigurationExpression.CreateMap<SidecarFileHash, SidecarFileHashDto>();
            _ = mapperConfigurationExpression.CreateMap<SidecarFileHashDto, SidecarFileHash>();

            _ = mapperConfigurationExpression.CreateMap<FileVariations, FileVariationsDto>();
            _ = mapperConfigurationExpression.CreateMap<FileVariationsDto, FileVariations>();
        });
        return new Mapper(config);
    }
}