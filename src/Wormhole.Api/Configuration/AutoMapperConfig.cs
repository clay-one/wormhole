using AutoMapper;
using AutoMapper.Configuration;
using Wormhole.Api.Model;
using Wormhole.DomainModel;
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
            });
            Mapping.AutoMapper.Mapper = mapperConfiguration.CreateMapper();
        }
    }
}