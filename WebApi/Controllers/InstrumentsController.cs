using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.EntityModels;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Repositories;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.Instruments)]
    public class InstrumentsController : Controller
    {
        private InstrumentRepository repository;
        private const string PostActionName="PostInstrument";

        // GET: api/Instruments
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Instrument>))]
        
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.FindInstruments());
        }

        // GET: api/Instruments/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Instrument))]
        
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.FindInstruments(x=>x.ID==id));
        }
        
        // POST: api/Instruments
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(Instrument))]
        public async Task<IActionResult> Post([FromBody]Instrument value)
        {
            return Created(PostActionName, await repository.AddInstrument(value));
        }
        
        // PUT: api/Instruments/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]Instrument value)
        {
             throw new NotImplementedException();
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public InstrumentsController(InstrumentRepository repository)
        {
            this.repository = repository;
        }
    }
}
