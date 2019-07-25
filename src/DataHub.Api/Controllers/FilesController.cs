using Amazon.S3.Model;
using DataHub.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace DataHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private const string BUCKET_NAME = "datahub69";
        private const int PAGE_SIZE = 10;

        private readonly IFileServices _fileServices;
        public FilesController(IFileServices fileServices)
        {
            _fileServices = fileServices;
        }

        [HttpPost]
        public async Task<IActionResult> GetListingAsync(ListObjectsV2Request request)
        {
            var response = await _fileServices.GetListAsync(request);

            return Ok(response);
        }

        [HttpGet]
        [Route("{key}/download")]
        public async Task<IActionResult> DownloadAsync(string key)
        {
            var stream = await _fileServices.DownloadAsync(key);

            return File(stream, "application/octet-stream");
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload([FromForm]IFormFile file)
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                await _fileServices.CreateAsync(file.FileName, stream);
                return Ok();
            }
        }
    }
}