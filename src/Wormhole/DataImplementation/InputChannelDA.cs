using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public class InputChannelDA : IInputChannelDA
    {
        private IMongoCollection<InputChannel> InputChannelCollection
            => MongoUtil.GetCollection<InputChannel>(nameof(InputChannel));
        public async Task AddInputChannelAsync(InputChannel channel)
        {
            await InputChannelCollection.InsertOneAsync(channel);
        }
    }
}