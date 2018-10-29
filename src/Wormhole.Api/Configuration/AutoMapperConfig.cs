using AutoMapper;
using Wormhole.Mapping.Profile;

namespace Wormhole.Api.Configuration
{
    public static class AutoMapperConfig
    {
        public static void ConfigureAllMappers()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.AddProfile(new InputChannelAddRequestProfile());
                config.AddProfile(new InputChannelAddResponseProfile());
            });
            Mapping.AutoMapper.Mapper = mapperConfiguration.CreateMapper();
        }
    }
}