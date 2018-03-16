using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.EntityModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route("api/Test")]
    public class TestController : Controller
    {
        private const string PostActionName="testap";

        // GET: api/Test
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<object>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/Test/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(object))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Test
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(AccountSummary))]
        public async Task<IActionResult> Post([FromBody]object value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/Test/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]object value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }
    }
}
