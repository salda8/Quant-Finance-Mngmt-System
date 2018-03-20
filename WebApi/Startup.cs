using CommonStandard.Interfaces;
using DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ServerStandard.Repositories;
using Swashbuckle.AspNetCore.Swagger;
using System;
using WebApi.Controllers;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = data; Integrated Security = True; Connect Timeout = 30; Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger
            // JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            app.UseResponseCaching();
            app.Use(async (context, next) =>
    {
        context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
        {
            Public = true,
            MaxAge = TimeSpan.FromSeconds(10)
        };
        context.Response.Headers[HeaderNames.Vary] = new[] { "Accept-Encoding" };

        await next();
    });

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<DataDBContext>(opts => opts.UseSqlServer(Configuration["ConnectionString:DataDb"]));

            services.AddDbContext<MyDBContext>(opts => opts.UseSqlServer(Configuration["ConnectionString:MyDb"]));

            services.AddScoped<IMyDbContext, MyDBContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddDistributedRedisCache(options =>
   {
       options.Configuration = "localhost:6379"; //location of redis server
   });
            services.AddScoped<ETagCache>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddResponseCaching();
            services.AddMvc(options =>
     {   //In Core 2.1 - We are adding the [ApiController] attribute as the way to opt-in to Web API specific conventions and behaviors. These behaviors include:
         //Automatically responding with a 400 when validation errors occur
         //Infer smarter defaults for action parameters: [FromBody] for complex types, [FromRoute] when possible, otherwise [FromQuery]
         //Requires attribute routing – actions are not accessible by convention-based routes
         options.Filters.Add(typeof(ModelStateValidationFilter));
     });
            services.AddSwaggerGen(c =>
   {
       c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
   });
        }
    }
}