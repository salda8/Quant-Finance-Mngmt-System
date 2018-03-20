using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Controllers
{
    public class MySwaggerResponseAttribute : SwaggerResponseAttribute
    {
        public MySwaggerResponseAttribute(HttpStatusCode statusCode, Type type = null, string description = null) : base((int)statusCode, type, description)
        {

        }
    }
}