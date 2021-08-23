using CleanSolution.Core.Application;
using CleanSolution.Infrastructure.Files;
using CleanSolution.Infrastructure.Persistence;
using CleanSolution.Presentation.WebApi.Extensions;
using CleanSolution.Presentation.WebApi.Extensions.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Workabroad.Infrastructure.Logger;
using Workabroad.Presentation.WebApi.Extensions.Middlewares;

namespace CleanSolution.Presentation.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => this.Configuration = configuration;


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddThisLayer(Configuration);

            services.AddApplicatonLayer(Configuration);

            services.AddFilesLayer(Configuration);
            services.AddLoggerLayer(Configuration);
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
