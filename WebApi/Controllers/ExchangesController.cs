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
    [Produces("application/json"), Route(ApiRoutes.Exchanges)]
    public class ExchangesController : Controller
    {
        // GET: api/Exchanges
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Exchange>))]
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/Exchanges/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Exchange))]
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Exchanges
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Exchange value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/Exchanges/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]Exchange value)
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
