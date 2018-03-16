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
    [Produces("application/json"), Route(ApiRoutes.CommissionMessage)]
    public class CommissionMessageController : Controller
    {
        private readonly IGenericRepository<CommissionMessage> repository;
        private const string PostActionName="PostCommissionMessage";

        // GET: api/CommissionMessage
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<CommissionMessage>))]
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/CommissionMessage/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(CommissionMessage))]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/CommissionMessage
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(CommissionMessage))]
        public async Task<IActionResult> Post([FromBody]CommissionMessage value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/CommissionMessage/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(CommissionMessage))]
        public async Task<IActionResult> Put(int id, [FromBody]CommissionMessage value)
        {
            return Ok(await repository.Update(value));
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public CommissionMessageController(IGenericRepository<CommissionMessage> repository)
        {
            this.repository = repository;
        }
    }
}
