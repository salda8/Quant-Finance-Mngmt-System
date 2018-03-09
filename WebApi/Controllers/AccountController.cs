using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Common.EntityModels;
using Common.Interfaces;
using CommonStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Repositories;
using ServerStandard.Repositories;

namespace WebApi.Controllers
{
    [Produces("application/json"), Route(ApiRoutes.Account)]
    public class AccountController : Controller
    {
        private const string PostActionName = "PostAccount";

        // GET: api/Account
        [HttpGet, MySwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Account>))]
        public async Task<IActionResult> Get()
        {
           return Ok(await repository.Get());

        }

    // GET: api/Account/5
        [HttpGet("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Get(int id)
    {
        return Ok(await repository.GetByID(id));
    }
        
        // POST: api/Account
        [HttpPost(Name= PostActionName), MySwaggerResponse(HttpStatusCode.Created, typeof(Account))]
        public async Task<IActionResult> Post([FromBody]Account value)
        {
            return Created(PostActionName, await repository.Insert(value));
        }
        
        // PUT: api/Account/5
        [HttpPut("{id}"), MySwaggerResponse(HttpStatusCode.OK, typeof(Account))]
        public async Task<IActionResult> Put(int id, [FromBody]Account value)
        {
          return  Ok(await repository.Update(value));
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}"), MySwaggerResponse(HttpStatusCode.Accepted)]
        public async Task<IActionResult> Delete(int id)
        {
            repository.Delete(id);
            return Accepted();
        }


        public AccountController(IGenericRepository<Account> repository)
        {
            this.repository = repository;
        }

        private readonly IGenericRepository<Account> repository;
    }
    }

