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
    [Produces("application/json"), Route(ApiRoutes.Strategy)]
    public class StrategyController : Controller
    {
        // GET: api/Strategy
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Strategy>))]
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/Strategy/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Strategy))]
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Strategy
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Strategy value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/Strategy/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]Strategy value)
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
