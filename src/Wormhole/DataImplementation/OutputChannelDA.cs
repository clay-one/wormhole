using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public class OutputChannelDa : IOutputChannelDa
    {
        private readonly IMongoUtil _mongoUtil;
        private IMongoCollection<OutputChannel> OutputChannelCollection
            => _mongoUtil.GetCollection<OutputChannel>(nameof(OutputChannel));

        public OutputChannelDa(IMongoUtil mongoUtil)
        {
            _mongoUtil = mongoUtil;
        }
        public async Task<List<OutputChannel>> FindOutputChannels()
        {
            return await OutputChannelCollection.Find(Builders<OutputChannel>.Filter.Empty).ToListAsync();
        }

        public async Task SetJobId(string id, string jobId)
        {
            var filter = Builders<OutputChannel>.Filter.Eq(nameof(OutputChannel.Id),new ObjectId(id));

            var update = Builders<OutputChannel>.Update.Set(nameof(OutputChannel.JobId), jobId);
            await OutputChannelCollection.UpdateOneAsync(filter, update);
        }

        public async Task AddOutputChannel(OutputChannel channel)
        {
            await OutputChannelCollection.InsertOneAsync(channel);
        }
    }
}