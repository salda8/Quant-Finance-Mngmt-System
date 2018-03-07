using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.EntityModels;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.TradeHistory)]
    public class TradeHistoryController : Controller
    {
        // GET: api/TradeHistory
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<TradeHistory>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/TradeHistory/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(TradeHistory))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/TradeHistory
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TradeHistory value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/TradeHistory/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]TradeHistory value)
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
