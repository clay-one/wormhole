using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public class MessageLogDa : IMessageLogDa
    {
        private readonly IMongoUtil _mongoUtil;

        
        private IMongoCollection<OutgoingMessageLog> MessageLogCollection
            => _mongoUtil.GetCollection<OutgoingMessageLog>(nameof(OutgoingMessageLog));

        public MessageLogDa(IMongoUtil mongoUtil)
        {
            _mongoUtil = mongoUtil;
        }

        public async Task AddAsync(OutgoingMessageLog outgoingMessageLog)
        {
            await MessageLogCollection.InsertOneAsync(outgoingMessageLog);
        }
    }
}