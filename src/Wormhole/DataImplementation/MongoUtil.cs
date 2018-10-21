using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Wormhole.Utils;

namespace Wormhole
{
    public class MongoUtil
    {
        private static MongoClient _client;
        private static IMongoDatabase _database;
        private static bool IsInitialized => _client != null && _database != null;

        public static IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            if (!IsInitialized)
                Initialize();

            var collection = _database.GetCollection<T>(collectionName);
            return collection;
        }

        private static void Initialize()
        {
            var connectionString = GetConnectionString();

            var mongoUrl = MongoUrl.Create(connectionString);
            var databaseName = mongoUrl.DatabaseName;

            _client = new MongoClient(mongoUrl);
            _database = _client.GetDatabase(databaseName);
        }

        private static string GetConnectionString()
        {
            return AppSettingsProvider.MongoConnectionString;
        }

        public static void CreateIndex<T>(string name, IndexKeysDefinition<T> keys, CreateIndexOptions options = null)
        {
            var collection = GetCollection<T>(typeof(T).Name);

            var foundIndexes = FindIndexByName(collection, name);
            if (foundIndexes.Any())
                return;

            if (options == null)
                options = new CreateIndexOptions();

            options.Name = name;
            collection.Indexes.CreateOne(keys, options);
        }

        public static void DropIndex<T>(string name)
        {
            var collection = GetCollection<T>(typeof(T).Name);

            var foundIndexes = FindIndexByName(collection, name);
            if (foundIndexes.Any())
                collection.Indexes.DropOne(name);
        }

        private static IEnumerable<BsonDocument> FindIndexByName<T>(IMongoCollection<T> collection, string name)
        {
            return collection.Indexes.List().ToList().Where(ff => ff.Values.Where(hh => hh.BsonType == BsonType.String)
                .Any(gg => gg.AsString == name));
        }
    }
}