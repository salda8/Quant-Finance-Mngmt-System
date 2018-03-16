using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommonStandard;
using Common.EntityModels;
using Server.Repositories;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.ExecutionMessage)]
    public class ExecutionMessageController : Controller
    {
        private IGenericRepository<ExecutionMessage> repository;private const string PostActionName="PostExecutionMessage";

        // GET: api/ExecutionMessage
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<ExecutionMessage>))]
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/ExecutionMessage/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(ExecutionMessage))]
        
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/ExecutionMessage
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(ExecutionMessage))]
        public async Task<IActionResult> Post([FromBody]ExecutionMessage value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/ExecutionMessage/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]ExecutionMessage value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public ExecutionMessageController(IGenericRepository<ExecutionMessage> repository)
        {
            this.repository = repository;
        }
    }
}
