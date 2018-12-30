using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public class MessageLogDa : IMessageLogDa
    {
        private IMongoCollection<OutgoingMessageLog> MessageLogCollection
            => MongoUtil.GetCollection<OutgoingMessageLog>(nameof(OutgoingMessageLog));
        

        public async Task AddAsync(OutgoingMessageLog outgoingMessageLog)
        {
            await MessageLogCollection.InsertOneAsync(outgoingMessageLog);
        }
    }
}