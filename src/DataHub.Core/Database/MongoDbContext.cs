using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace DataHub.Core.Database
{
    public class MongoDbContext : IMongoDbContext
    {
        public MongoClient Client { get; }
        public IMongoDatabase Database { get; }

        public MongoDbContext(string connectionString, string databaseName)
        {
            Client = new MongoClient(connectionString);
            Database = Client.GetDatabase(databaseName);
            Database.GetCollection<BsonDocument>("contacts")
                .Indexes
                .CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("SDT"), new CreateIndexOptions
                {
                    Unique = true,
                }));                
        }

        public IMongoCollection<DataFile> Files => Database.GetCollection<DataFile>("files");
        public IMongoCollection<BsonDocument> Contacts => Database.GetCollection<BsonDocument>("contacts");
    }

    public class DataFile
    {
        [BsonId]
        public ObjectId FileId { get; set; }

        public string FileName { get; set; }

        public string Status { get; set; }

        public long CreatedAt { get; set; }
    }
}
