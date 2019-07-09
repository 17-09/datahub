using DataHub.Core.Database;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataHub.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IMongoDbContext _mongo;
        public ContactsController(IMongoDbContext mongo)
        {
            _mongo = mongo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contacts = _mongo.Contacts.AsQueryable()
                .Take(20)
                .Skip(0);

            return Ok(contacts);
        }
    }
}
