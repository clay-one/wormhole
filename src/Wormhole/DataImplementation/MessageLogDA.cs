using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public class MessageLogDa : IMessageLogDa
    {
        private IMongoCollection<MessageLog> MessageLogCollection
            => MongoUtil.GetCollection<MessageLog>(nameof(MessageLog));
        

        public async Task AddAsync(MessageLog messageLog)
        {
            await MessageLogCollection.InsertOneAsync(messageLog);
        }
    }
}