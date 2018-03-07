using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.EntityModels;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Controllers
{
   
    [Produces("application/json"), Route(ApiRoutes.LiveTrades)]
    public class LiveTradesController : Controller
    {
        // GET: api/LiveTrades
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<LiveTrade>), "Suucess get result." )]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/LiveTrades/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(LiveTrade))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/LiveTrades
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]LiveTrade value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/LiveTrades/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]LiveTrade value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }
    }
}
