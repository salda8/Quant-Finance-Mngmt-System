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
    [Produces("application/json"), Route(ApiRoutes.SessionTemplates)]
    public class SessionTemplateController : Controller
    {
        private IGenericRepository<SessionTemplate> repository;private const string PostActionName="PostSessionTemplate";

        // GET: api/SessionTemplate
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<SessionTemplate>))]
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/SessionTemplate/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(SessionTemplate))]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/SessionTemplate
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(SessionTemplate))]
        public async Task<IActionResult> Post([FromBody]SessionTemplate value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/SessionTemplate/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]SessionTemplate value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public SessionTemplateController(IGenericRepository<SessionTemplate> repository)
        {
            this.repository = repository;
        }
    }
}
