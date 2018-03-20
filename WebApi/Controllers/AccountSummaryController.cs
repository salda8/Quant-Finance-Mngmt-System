using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Common.EntityModels;
using CommonStandard;
using CommonStandard.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Repositories;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.AccountSummary)]
    public class AccountSummaryController : Controller
    {
        private readonly IGenericRepository<AccountSummary> repository;
        private readonly ETagCache cache;
        private const string PostActionName= "PostAccountSummary";
        private const string CacheKey = "accountSummary";   

        // GET: api/AccountSummary
        [HttpGet("get"), MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<AccountSummary>))]
        [ResponseCache(Duration = 1800)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await repository.Get());
        }

        // GET: api/AccountSummary
        [HttpGet("get/filtered"), MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<AccountSummary>))]
        public async Task<IActionResult> GetAllFiltered(Expression<Func<AccountSummary, bool>> filter = null,
           Dictionary<string,OrderingOrder> orderBy = null,
            string includeProperties = "")
        {
            return Ok(await repository.Get());
        }


        // GET: api/AccountSummary/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(AccountSummary)), MySwaggerResponse(HttpStatusCode.NotModified, null, "Return a 304 if the ETag of the current record matches the ETag in the 'If - None - Match' HTTP header")]
        public async Task<IActionResult> Get(int id)
        {
          
            var objectToReturn = cache.GetCachedObject<AccountSummary>($"{CacheKey}-{id}");
            if (objectToReturn == null)
            {
                objectToReturn = await repository.GetByID(id);
                if (objectToReturn==null)
                {
                    return NotFound();
                }
            }

            bool isModified = cache.SetCachedObject($"{CacheKey}-{id}", objectToReturn);

            if (isModified)
            {
                return Ok(objectToReturn);
            }

            return StatusCode((int)HttpStatusCode.NotModified);
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

        public AccountSummaryController(IGenericRepository<AccountSummary> repository, ETagCache cache)
        {
            this.repository = repository;
            this.cache = cache;
        }
    }
}
