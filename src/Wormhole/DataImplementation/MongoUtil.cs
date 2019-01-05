using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Wormhole.Configurations;

namespace Wormhole.DataImplementation
{
    public class MongoUtil : IMongoUtil
    {
        private static MongoClient _client;
        private static IMongoDatabase _database;
        private readonly string _connectionString;

        public MongoUtil(ILoggerFactory loggerFactory, IOptions<ConnectionStringsConfig> options)
        {
            Logger = loggerFactory.CreateLogger(nameof(MongoUtil));
            _connectionString = options.Value.Mongo;
        }
        
        private ILogger Logger { get; set; }

        private bool IsInitialized => _client != null && _database != null;

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            var collection = _database.GetCollection<T>(collectionName);
            return collection;
        }

        private void Initialize()
        {
            try
            {
                var mongoUrl = MongoUrl.Create(_connectionString);
                var databaseName = mongoUrl.DatabaseName;

                _client = new MongoClient(mongoUrl);
                _database = _client.GetDatabase(databaseName);
            }

            catch (MongoConfigurationException ex)
            {
                Logger.LogError($"MongoDB Configuration ERROR: {ex.Message}",
                    ex);
                throw;
            }
        }



        public void CreateIndex<T>(string name, IndexKeysDefinition<T> keys, CreateIndexOptions options = null)
        {
            var collection = GetCollection<T>(typeof(T).Name);

            var foundIndexes = FindIndexByName(collection, name);
            if (foundIndexes.Any())
            {
                return;
            }

            if (options == null)
            {
                options = new CreateIndexOptions();
            }

            options.Name = name;

            var createIndexModel = new CreateIndexModel<T>(keys, options);

            collection.Indexes.CreateOne(createIndexModel);
        }

        public void DropIndex<T>(string name)
        {
            var collection = GetCollection<T>(typeof(T).Name);

            var foundIndexes = FindIndexByName(collection, name);
            if (foundIndexes.Any())
            {
                collection.Indexes.DropOne(name);
            }
        }

        private static IEnumerable<BsonDocument> FindIndexByName<T>(IMongoCollection<T> collection, string name)
        {
            return collection.Indexes
                .List()
                .ToList()
                .Where(ff => ff.Values.Where(hh => hh.BsonType == BsonType.String)
                    .Any(gg => gg.AsString == name));
        }
    }
}