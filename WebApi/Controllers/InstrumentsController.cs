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
    [Produces("application/json"), Route(ApiRoutes.Instruments)]
    public class InstrumentsController : Controller
    {
        // GET: api/Instruments
        [HttpGet, Produces(typeof(IEnumerable<Instrument>))]
        
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/Instruments/5
        [HttpGet("{id}"), Produces(typeof(Instrument))]
        
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/Instruments
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Instrument value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/Instruments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]Instrument value)
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
