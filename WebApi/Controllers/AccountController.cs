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
    [Produces("application/json"), Route(ApiRoutes.Account)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(200)]

    public class AccountController : Controller
    {
        // GET: api/Account
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Account>))]
        public async Task<IActionResult> Get()
        {
            throw new NotImplementedException();
        }

        // GET: api/Account/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Account
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Account value)
        {
            throw new NotImplementedException();
        }
        
        // PUT: api/Account/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]Account value)
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
