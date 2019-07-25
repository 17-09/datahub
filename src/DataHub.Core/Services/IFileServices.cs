using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;

namespace DataHub.Core.Services
{
    public interface IFileServices
    {
        Task CreateAsync(string fileName, Stream stream);
        void ProcessFile(string fileName);
        Task<ListObjectsV2Response> GetListAsync(ListObjectsV2Request request);
        Task<Stream> DownloadAsync(string key);
    }
}