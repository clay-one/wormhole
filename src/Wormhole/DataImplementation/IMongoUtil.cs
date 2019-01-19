using MongoDB.Driver;

namespace Wormhole.DataImplementation
{
    public interface IMongoUtil
    {
        void DropIndex<T>(string name);
        void CreateIndex<T>(string name, IndexKeysDefinition<T> keys, CreateIndexOptions options = null);
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}