using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.EntityModels;
using Common.Interfaces;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Repositories;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.Data)]
    public class DataController<T> : Controller where T : OHLCBar
    {
        private IGenericRepository<OHLCBar> repository;private const string PostActionName="PostOHLCbar";

        // GET: api/Data
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<OHLCBar>))]
        
        public async Task<IActionResult> Get()
        {
            return Ok(await repository.Get());
        }

        // GET: api/Data/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(OHLCBar))]
        
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await repository.GetByID(id));
        }
        
        // POST: api/Data
         [HttpPost(Name = PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(OHLCBar))]
        public async Task<IActionResult> Post([FromBody]T value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/Data/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(OHLCBar))]
        public async Task<IActionResult> Put(int id, [FromBody]T value)
        {
            return Ok(await repository.Update(value));
        }
        
        // DELETE: api/ApiWithActions/5
         [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
             throw new NotImplementedException();
        }

        public DataController(IGenericRepository<OHLCBar> repository)
        {
            this.repository = repository;
        }
    }
}
