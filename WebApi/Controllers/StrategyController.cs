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
    [Produces("application/json"), Route(ApiRoutes.Strategy)]
    public class StrategyController : Controller
    {
        private IGenericRepository<Strategy> repository;private const string PostActionName="PostStrategy";

        // GET: api/Strategy
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Strategy>))]
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/Strategy/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Strategy))]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/Strategy
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(Strategy))]
        public async Task<IActionResult> Post([FromBody]Strategy value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/Strategy/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]Strategy value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public StrategyController(IGenericRepository<Strategy> repository)
        {
            this.repository = repository;
        }
    }
}
