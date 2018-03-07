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
    [Produces("application/json"), Route(ApiRoutes.AccountSummary)]
    public class AccountSummaryController : Controller
    {
        // GET: api/AccountSummary
        [HttpGet("get"), MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<AccountSummary>))]
        public async Task<IActionResult> GetAll()
        {
            throw new NotImplementedException();
        }
        
        
        // GET: api/AccountSummary/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(AccountSummary))]
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }

        // POST: api/AccountSummary
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]AccountSummary value)
        {
            throw new NotImplementedException();
        }

        // PUT: api/AccountSummary/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]AccountSummary value)
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
