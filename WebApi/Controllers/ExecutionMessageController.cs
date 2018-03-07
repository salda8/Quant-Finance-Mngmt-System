using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommonStandard;
using Common.EntityModels;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.ExecutionMessage)]
    public class ExecutionMessageController : Controller
    {
        // GET: api/ExecutionMessage
        [HttpGet, Produces(typeof(IEnumerable<ExecutionMessage>))]
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/ExecutionMessage/5
        [HttpGet("{id}"), Produces(typeof(ExecutionMessage))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/ExecutionMessage
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ExecutionMessage value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/ExecutionMessage/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]ExecutionMessage value)
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
