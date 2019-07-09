using System.IO;
using System.Threading.Tasks;

namespace DataHub.Core.Services
{
    public interface IFileServices
    {
        Task CreateAsync(string fileName, Stream stream);
        void ProcessFile(string fileName);
    }
}