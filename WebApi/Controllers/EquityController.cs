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
    [Produces("application/json"), Route(ApiRoutes.Equity)]
    public class EquityController : Controller
    {
        private IGenericRepository<Equity> repository;private const string PostActionName="PostEquity";

        // GET: api/Equity
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Equity>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/Equity/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Equity))]
        
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/Equity
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(Equity))]
        public async Task<IActionResult> Post([FromBody]Equity value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/Equity/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]Equity value)
        {
            return Ok(await repository.Update(value));
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public EquityController(IGenericRepository<Equity> repository)
        {
            this.repository = repository;
        }
    }
}
