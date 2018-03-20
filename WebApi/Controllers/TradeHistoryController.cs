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
    [Produces("application/json"), Route(ApiRoutes.TradeHistory)]
    public class TradeHistoryController : Controller
    {
        private IGenericRepository<TradeHistory> repository;private const string PostActionName="PostTradeHistory";

        // GET: api/TradeHistory
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<TradeHistory>))]
        
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/TradeHistory/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(TradeHistory))]
        
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/TradeHistory
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(TradeHistory))]
        public async Task<IActionResult> Post([FromBody]TradeHistory value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/TradeHistory/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]TradeHistory value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public TradeHistoryController(IGenericRepository<TradeHistory> repository)
        {
            this.repository = repository;
        }
    }
}
