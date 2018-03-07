using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.EntityModels;
using Common.Interfaces;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.Data)]
    public class DataController<T> : Controller where T : OHLCBar
    {
        // GET: api/Data
        [HttpGet, Produces(typeof(IEnumerable<OHLCBar>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/Data/5
        [HttpGet("{id}"), Produces(typeof(OHLCBar))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Data
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]T value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/Data/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]T value)
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
