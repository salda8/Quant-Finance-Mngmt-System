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
    [Produces("application/json"), Route(ApiRoutes.SessionTemplates)]
    public class SessionTemplateController : Controller
    {
        // GET: api/SessionTemplate
        [HttpGet, Produces(typeof(IEnumerable<SessionTemplate>))]
        public async Task<IActionResult> Get()
        {
           throw new NotImplementedException();
        }

        // GET: api/SessionTemplate/5
        [HttpGet("{id}"), Produces(typeof(SessionTemplate))]
        public async Task<IActionResult> Get(int id)
        {
            throw new NotImplementedException();
        }
        
        // POST: api/SessionTemplate
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]SessionTemplate value)
        {
             throw new NotImplementedException();
        }
        
        // PUT: api/SessionTemplate/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]SessionTemplate value)
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
