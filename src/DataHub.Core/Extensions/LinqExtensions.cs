using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataHub.Core.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException($"{source} can't be null at {nameof(LinqExtensions)}.{nameof(ForEach)}");
            }

            foreach (var item in source)
            {
                action(item);
            }

            return source;
        }

        public static async Task<IEnumerable<T>> ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException($"{source} can't be null at {nameof(LinqExtensions)}.{nameof(ForEach)}");
            }

            foreach (var item in source)
            {
                await action(item);
            }

            return source;
        }
    }

    public static class MongoExtensions
    {
        public static BsonDocument MergeNonNull(this BsonDocument source, BsonDocument target)
        {
            var updators = target.Where(v => !v.Value.IsBsonNull)
                .Where(v => !string.IsNullOrEmpty(v.Value.ToString()));
            foreach (var u in updators)
            {
                source.Set(u.Name, u.Value);
            }
            return source;
        }
    }
}
