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
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Controllers
{
   
    [Produces("application/json"), Route(ApiRoutes.LiveTrades)]
    public class LiveTradesController : Controller
    {
        private IGenericRepository<LiveTrade> repository;private const string PostActionName="PostLiveTrade";

        // GET: api/LiveTrades
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<LiveTrade>), "Suucess get result." )]
        
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/LiveTrades/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(LiveTrade))]
        
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/LiveTrades
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(LiveTrade))]
        public async Task<IActionResult> Post([FromBody]LiveTrade value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/LiveTrades/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]LiveTrade value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public LiveTradesController(IGenericRepository<LiveTrade> repository)
        {
            this.repository = repository;
        }
    }
}
