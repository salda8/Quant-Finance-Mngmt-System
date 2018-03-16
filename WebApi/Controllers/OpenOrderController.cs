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
    [Produces("application/json"), Route(ApiRoutes.OpenOrders)]
    public class OpenOrderController : Controller
    {
        private IGenericRepository<OpenOrder> repository;private const string PostActionName="PostAccountSummary";

        // GET: api/OpenOrder
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<OpenOrder>))]
        
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/OpenOrder/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(OpenOrder))]
        
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/OpenOrder
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(OpenOrder))]
        public async Task<IActionResult> Post([FromBody]OpenOrder value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/OpenOrder/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]OpenOrder value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public OpenOrderController(IGenericRepository<OpenOrder> repository)
        {
            this.repository = repository;
        }
    }
}
