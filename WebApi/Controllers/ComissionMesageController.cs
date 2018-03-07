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
    [Produces("application/json"), Route(ApiRoutes.CommissionMessage)]
    public class CommissionMessageController : Controller
    {
        // GET: api/CommissionMessage
        [HttpGet, Produces(typeof(IEnumerable<CommissionMessage>))]
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/CommissionMessage/5
        [HttpGet("{id}"), Produces(typeof(CommissionMessage))]
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/CommissionMessage
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CommissionMessage value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/CommissionMessage/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]CommissionMessage value)
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
