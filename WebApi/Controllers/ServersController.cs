using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommonStandard;
using Common.EntityModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.Servers)]
    public class ServersController : Controller
    {
        // GET: api/Servers
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<object>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/Servers/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(object))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Servers
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]object value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/Servers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]object value)
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
