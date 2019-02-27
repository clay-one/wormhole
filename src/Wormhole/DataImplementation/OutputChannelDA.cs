using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Wormhole.DomainModel;
using Wormhole.DomainModel.OutputChannel;

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
        public async Task<List<OutputChannel>> FindAsync()
        {
            return await OutputChannelCollection.Find(Builders<OutputChannel>.Filter.Empty).ToListAsync();
        }

        public async Task<OutputChannel> FindAsync(string externalKey)
        {
            var filter = Builders<OutputChannel>.Filter.Eq(a => a.ExternalKey , externalKey);
            return await  OutputChannelCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task SetJobId(string externalKey, string jobId)
        {
            var filter = Builders<OutputChannel>.Filter.Eq(nameof(OutputChannel.ExternalKey),externalKey);

            var update = Builders<OutputChannel>.Update.Set(nameof(OutputChannel.JobId), jobId);
            await OutputChannelCollection.UpdateOneAsync(filter, update);
        }

        public async Task AddOutputChannel(OutputChannel channel)
        {
            await OutputChannelCollection.InsertOneAsync(channel);
        }
    }
}