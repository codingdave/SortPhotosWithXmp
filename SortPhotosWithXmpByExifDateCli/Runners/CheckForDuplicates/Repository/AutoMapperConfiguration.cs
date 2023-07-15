using AutoMapper;
using SortPhotosWithXmpByExifDateCli.Entities;

namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates.Store
{
    public class AutoMapperConfiguration
    {
        public static Mapper InitializeAutomapper()
        {
            var config = new MapperConfiguration(mapperConfigurationExpression =>
            {
                mapperConfigurationExpression.CreateMap<XmpHash, XmpHashDto>();
                mapperConfigurationExpression.CreateMap<XmpHashDto, XmpHash>();

                mapperConfigurationExpression.CreateMap<ImageHash, ImageHashDto>();
                mapperConfigurationExpression.CreateMap<ImageHashDto, ImageHash>();
            });
            return new Mapper(config);
        }
    }
}