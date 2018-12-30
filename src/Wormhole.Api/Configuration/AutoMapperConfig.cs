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

                config.AddProfile(new HttpPushOutputChannelAddRequestProfile());
                config.AddProfile(new KafkaOutputChannelAddRequestProfile());
                config.AddProfile(new HttpPushOutputChannelAddResponseProfile());
                config.AddProfile(new KafkaOutputChannelAddResponseProfile());

                config.AddProfile(new AddTenantRequestProfile());
                config.AddProfile(new AddTenantResponseProfile());
            });
            Mapping.AutoMapper.Mapper = mapperConfiguration.CreateMapper();
        }
    }
}