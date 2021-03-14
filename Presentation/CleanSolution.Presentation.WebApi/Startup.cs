using CleanSolution.Core.Application;
using CleanSolution.Infrastructure.Files;
using CleanSolution.Infrastructure.Persistence;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Workabroad.Presentation.Admin.Extensions.Services;

namespace CleanSolution.Presentation.WebApi
{
    public class Startup
    {
        public IConfiguration configuration { get; }
        public Startup(IConfiguration configuration) => this.configuration = configuration;


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddFluentValidation();

            services.AddSwaggerServices("CleanSolution v1");

            services.AddApplicatonLayer(configuration);
            services.AddFilesLayer(configuration);
            services.AddPersistenceLayer(configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerMiddleware("CleanSolution v1");
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
