using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.EntityModels;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Repositories;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.Exchanges)]
    public class ExchangesController : Controller
    {
        private IGenericRepository<Exchange> repository;private const string PostActionName="PostExchange";

        // GET: api/Exchanges
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Exchange>))]
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/Exchanges/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Exchange))]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/Exchanges
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(Exchange))]
        public async Task<IActionResult> Post([FromBody]Exchange value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/Exchanges/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]Exchange value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public ExchangesController(IGenericRepository<Exchange> repository)
        {
            this.repository = repository;
        }
    }
}
