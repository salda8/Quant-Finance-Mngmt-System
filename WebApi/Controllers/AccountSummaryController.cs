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
    [Produces("application/json"), Route(ApiRoutes.AccountSummary)]
    public class AccountSummaryController : Controller
    {
        private readonly IGenericRepository<AccountSummary> repository;
        private const string PostActionName= "PostAccountSummary";

        // GET: api/AccountSummary
        [HttpGet("get"), MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<AccountSummary>))]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await repository.Get());
        }
        
        
        // GET: api/AccountSummary/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(AccountSummary))]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }

        // POST: api/AccountSummary
        [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(AccountSummary))]
        public async Task<IActionResult> Post([FromBody]AccountSummary value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }

        // PUT: api/AccountSummary/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(AccountSummary))]
        public async Task<IActionResult> Put(int id, [FromBody]AccountSummary value)
        {
            return Ok(await repository.Update(value));
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public AccountSummaryController(IGenericRepository<AccountSummary> repository)
        {
            this.repository = repository;
        }
    }
}
