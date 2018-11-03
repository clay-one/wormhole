using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public class OutputChannelDa : IOutputChannelDa
    {
        private IMongoCollection<OutputChannel> OutputChannelCollection
            => MongoUtil.GetCollection<OutputChannel>(nameof(OutputChannel));
        public async Task<List<OutputChannel>> FindOutputChannels()
        {
            return await OutputChannelCollection.Find(Builders<OutputChannel>.Filter.Empty).ToListAsync();
        }

        public async Task AddOutputChannel(OutputChannel channel)
        {
            await OutputChannelCollection.InsertOneAsync(channel);
        }
    }
}