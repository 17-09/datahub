using MongoDB.Bson;
using MongoDB.Driver;

namespace DataHub.Core.Database
{
    public interface IMongoDbContext
    {
        IMongoCollection<DataFile> Files { get; }
        IMongoCollection<BsonDocument> Contacts { get; }
    }
}