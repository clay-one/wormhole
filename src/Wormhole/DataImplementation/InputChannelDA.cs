using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public class InputChannelDa : IInputChannelDa
    {
        private readonly IMongoUtil _mongoUtil;
        
        private IMongoCollection<InputChannel> InputChannelCollection
            => _mongoUtil.GetCollection<InputChannel>(nameof(InputChannel));

        public InputChannelDa(IMongoUtil mongoUtil)
        {
            _mongoUtil = mongoUtil;
        }

        public async Task AddInputChannelAsync(InputChannel channel)
        {
            await InputChannelCollection.InsertOneAsync(channel);
        }
    }
}