using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route("api/Test")]
    public class TestController : Controller
    {
        // GET: api/Test
        [HttpGet, Produces(typeof(IEnumerable<object>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/Test/5
        [HttpGet("{id}"), Produces(typeof(object))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Test
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]object value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/Test/5
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
