using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Produces("application/json")]
    [Route(ApiRoutes.AccountSummary)]
    public class AccountSummaryController : Controller
    {
        // GET: api/AccountSummary
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/AccountSummary/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/AccountSummary
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/AccountSummary/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
