﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Produces("application/json")]
    [Route(ApiRoutes.CommissionMessage)]
    public class CommissionMessageController : Controller
    {
        // GET: api/CommissionMessage
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/CommissionMessage/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/CommissionMessage
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/CommissionMessage/5
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
