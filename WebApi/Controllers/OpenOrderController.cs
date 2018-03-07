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
    [Produces("application/json"), Route(ApiRoutes.OpenOrders)]
    public class OpenOrderController : Controller
    {
        // GET: api/OpenOrder
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<OpenOrder>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/OpenOrder/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(OpenOrder))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/OpenOrder
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]OpenOrder value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/OpenOrder/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]OpenOrder value)
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
