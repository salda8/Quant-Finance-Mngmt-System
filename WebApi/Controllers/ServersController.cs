using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommonStandard;
using Common.EntityModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Repositories;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.Servers)]
    public class ServersController : Controller
    {
        //private IGenericRepository<Server> repository;private const string PostActionName="PostServer";

        // GET: api/Servers
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<object>))]
        
        public async Task<IActionResult> Get()
        {
            //return Ok(await repository.Get());
            throw new NotImplementedException();
        }

        // GET: api/Servers/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(object))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Servers
         //[HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(Server))]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]object value)
        {
            //return Created(PostActionName, await repository.Insert(value));
            throw new NotImplementedException();
        }
        
        // PUT: api/Servers/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]object value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        //public ServersController(IGenericRepository<Server> repository)
        //{
        //    this.repository = repository;
        //}
    }
}
