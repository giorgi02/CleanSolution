﻿using CleanSolution.Core.Application;
using CleanSolution.Core.Application.Interfaces.Contracts;
using CleanSolution.Infrastructure.Files;
using CleanSolution.Infrastructure.Persistence;
using CleanSolution.Presentation.WebApi.Extensions.Middlewares;
using CleanSolution.Presentation.WebApi.Extensions.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Workabroad.Presentation.Admin.Extensions.Middlewares;
using Workabroad.Presentation.Admin.Extensions.Services;

namespace CleanSolution.Presentation.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => this.Configuration = configuration;


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddFluentValidation();

            // todo: მივაქციო ყურადღება, ზოგგან მუშაობს ზოგგან არა
            services.AddHttpContextAccessor(); // IHttpContextAccessor -ის ინექციისთვის
            services.AddScoped<IActiveUserService, ActiveUserService>();

            services.AddConfigureCors();
            services.AddConfigureHealthChecks(Configuration);
            services.AddSwaggerServices("CleanSolution v1");


            services.AddApplicatonLayer(Configuration);
            services.AddFilesLayer(Configuration);
            services.AddPersistenceLayer(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerMiddleware("CleanSolution v1");
            }

            app.UseMiddleware<ExceptionHandler>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCors("CorsPolicy");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapCustomHealthCheck();

                endpoints.MapControllers();
            });
        }
    }
}
