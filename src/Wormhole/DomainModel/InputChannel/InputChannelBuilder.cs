using System;
using Wormhole.Api.Model;

namespace Wormhole.DomainModel.InputChannel
{
    public static class InputChannelBuilder
    {
        public static InputChannel CreateHttpPushInputChannel(HttpPushInputputChannelAddRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.TenantId) || string.IsNullOrWhiteSpace(input.ExternalKey))
                throw new ArgumentNullException();

            var channel = Mapping.AutoMapper.Mapper.Map<InputChannel>(input);
            channel.ChannelType = ChannelType.HttpPush;
            channel.TypeSpecification = new HttpPushInputChannelSpecification();
            return channel;
        }
    }
}