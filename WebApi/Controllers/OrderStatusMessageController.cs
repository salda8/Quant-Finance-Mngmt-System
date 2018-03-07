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
    [Produces("application/json"), Route(ApiRoutes.OrderStatusMessage)]
    public class OrderStatusMessageController : Controller
    {
        // GET: api/OrderStatusMessage
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<OrderStatusMessage>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/OrderStatusMessage/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(OrderStatusMessage))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/OrderStatusMessage
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]OrderStatusMessage value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/OrderStatusMessage/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]OrderStatusMessage value)
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
