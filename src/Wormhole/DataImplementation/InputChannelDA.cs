using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel.InputChannel;

namespace Wormhole.DataImplementation
{
    public class InputChannelDa : IInputChannelDa
    {
        private IMongoCollection<InputChannel> InputChannelCollection
            => MongoUtil.GetCollection<InputChannel>(nameof(InputChannel));

        public async Task AddInputChannelAsync(InputChannel channel)
        {
            await InputChannelCollection.InsertOneAsync(channel);
        }
    }
}