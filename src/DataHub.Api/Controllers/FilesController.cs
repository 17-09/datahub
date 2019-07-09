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
        private readonly IFileServices _fileServices;
        public FilesController(IFileServices fileServices)
        {
            _fileServices = fileServices;
        }

        [HttpPost]
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
