using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.EntityModels;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.Equity)]
    public class EquityController : Controller
    {
        // GET: api/Equity
        [HttpGet, Produces(typeof(IEnumerable<Equity>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/Equity/5
        [HttpGet("{id}"), Produces(typeof(Equity))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Equity
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Equity value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/Equity/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]Equity value)
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
