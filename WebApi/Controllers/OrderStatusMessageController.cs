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
    [Produces("application/json"), Route(ApiRoutes.OrderStatusMessage)]
    public class OrderStatusMessageController : Controller
    {
        private IGenericRepository<OrderStatusMessage> repository;private const string PostActionName="PostOrderStatusMsg";

        // GET: api/OrderStatusMessage
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<OrderStatusMessage>))]
        
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/OrderStatusMessage/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(OrderStatusMessage))]
        
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/OrderStatusMessage
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(OrderStatusMessage))]
        public async Task<IActionResult> Post([FromBody]OrderStatusMessage value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/OrderStatusMessage/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]OrderStatusMessage value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public OrderStatusMessageController(IGenericRepository<OrderStatusMessage> repository)
        {
            this.repository = repository;
        }
    }
}
