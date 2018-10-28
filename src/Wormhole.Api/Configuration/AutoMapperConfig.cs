using AutoMapper;

namespace Wormhole.Api.Configuration
{
    public static class AutoMapperConfig
    {
        public static void ConfigureAllMappers()
        {
            var mapperConfiguration = new MapperConfiguration(config => { });
            Mapping.AutoMapper.Mapper = mapperConfiguration.CreateMapper();
        }
    }
}