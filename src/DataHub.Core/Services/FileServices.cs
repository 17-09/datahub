using Amazon.S3;
using Amazon.S3.Model;
using CsvHelper;
using DataHub.Core.Database;
using DataHub.Core.Extensions;
using Hangfire;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DataHub.Core.Services
{
    public class FileServices : IFileServices
    {
        private const int BUFFER_SIZE = 5;
        private const string BUCKET_NAME = "datahub69";

        private readonly IAmazonS3 _s3;
        private readonly IMongoDbContext _mongo;
        public FileServices(IAmazonS3 s3, IMongoDbContext mongoDbContext)
        {
            _s3 = s3;
            _mongo = mongoDbContext;
        }

        public async Task CreateAsync(string fileName, Stream stream)
        {
            var key = DateTime.UtcNow.Ticks + "_" + fileName;
            stream.Position = 0;

            var response = await _s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = BUCKET_NAME,
                InputStream = stream,
                Key = key,
            });

            if (response.HttpStatusCode == HttpStatusCode.OK
                || response.HttpStatusCode == HttpStatusCode.Created)
            {
                await _mongo.Files.InsertOneAsync(new DataFile
                {
                    FileName = key,
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Status = "Created",
                });

                // Create job
                BackgroundJob.Enqueue(() => ProcessFile(key));
            }
            else
            {
                throw new Exception("Oops! We can not upload file to S3");
            }
        }

        public async Task<Stream> DownloadAsync(string key)
        {
            var response = await _s3.GetObjectAsync("datahub69", key);
            using (var stream = response.ResponseStream)
            {
                var memory = new MemoryStream();
                await stream.CopyToAsync(memory);

                memory.Position = 0;
                return memory;
            }
        }

        public async Task<ListObjectsV2Response> GetListAsync(ListObjectsV2Request request)
        {
            return await _s3.ListObjectsV2Async(request);
        }

        public void ProcessFile(string key)
        {
            var stream = GetRecords(key)
                .ToObservable()
                .Buffer(BUFFER_SIZE)
                .Select(StandardizedData)
                .Select(Separate)
                .Publish();

            stream.Subscribe(x => InsertMany(x.Inserts));
            stream.Subscribe(x => ReplaceMany(x.Updates));

            stream.Connect();
        }

        public void InsertMany(IEnumerable<BsonDocument> records)
        {
            if (!records.Any())
                return;

            _mongo.Contacts.InsertMany(records);
        }

        public void ReplaceMany(IEnumerable<BsonDocument> records)
        {
            foreach (var r in records)
            {
                _mongo.Contacts.ReplaceOne(c => c["SDT"] == r["SDT"], r);
            }
        }

        public IEnumerable<BsonDocument> StandardizedData(IEnumerable<BsonDocument> records)
        {
            foreach (var r in records)
            {
                // TODO: Standardized phone number
                var key = r["SDT"];

                if (!key.AsString.StartsWith('0'))
                {
                    r.Set("SDT", BsonValue.Create("0" + key));
                }
            }

            return records.Distinct(new ContactEqualityComparer());
        }

        public (IEnumerable<BsonDocument> Inserts, IEnumerable<BsonDocument> Updates) Separate(IEnumerable<BsonDocument> records)
        {
            var updates = GetExistRecords(records)
                .Select(s => s.MergeNonNull(records.First(r => r["SDT"] == s["SDT"])));
            var updateKeys = updates.Select(u => u["SDT"]);

            var inserts = records.Where(r => !updateKeys.Contains(r["SDT"]));

            return (inserts, updates);
        }

        public IEnumerable<BsonDocument> GetExistRecords(IEnumerable<BsonDocument> source)
        {
            var uniqueKeys = source.Select(s => s["SDT"]);
            var response = _mongo.Contacts.AsQueryable()
                .Where(c => uniqueKeys.Contains(c["SDT"]))
                .ToList();

            return response;
        }

        public IEnumerable<BsonDocument> GetRecords(string key)
        {
            var response = _s3.GetObjectAsync("datahub69", key).Result;
            using (var stream = response.ResponseStream)
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    yield return BsonDocument.Create((IDictionary<string, object>)csv.GetRecord<dynamic>());
                }
            }
        }
    }

    public class ContactEqualityComparer : EqualityComparer<BsonDocument>
    {
        public override bool Equals(BsonDocument x, BsonDocument y)
        {
            var keyX = x["SDT"].AsString;
            var keyY = y["SDT"].AsString;

            return keyX.Equals(keyY);
        }

        public override int GetHashCode(BsonDocument obj)
        {
            return obj["SDT"].AsString.GetHashCode();
        }
    }
}
