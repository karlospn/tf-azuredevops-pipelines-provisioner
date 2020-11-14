using System;
using System.IO;
using System.Text;
using BuildDefinitionProvider.WebApi.Services.Azure.Builds;
using BuildDefinitionProvider.WebApi.Services.Azure.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildDefinitionProvider.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Move to config
            services.AddSingleton<IAzureClient>(sp => new AzureClient(
                Configuration["AzureDevOpsTenant"],
                Configuration["AzureDevopsPAT"]));

            services.AddTransient<IBuildDefinitionService, BuildDefinitionService>();
            services.AddTransient<IVariableService, VariableService>();
            services.AddTransient<IVariableGroupService, VariableGroupService>();
            services.AddTransient<ITaskGroupService, TaskGroupService>();
            services.AddTransient<ITriggersService, TriggersService>();
            services.AddTransient<ITagsService, TagsService>();
            services.AddControllers().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.Use(async (context, next) =>
            {
                var initialBody = context.Request.Body;

                using (var bodyReader = new StreamReader(context.Request.Body))
                {
                    string body = await bodyReader.ReadToEndAsync();
                    Console.WriteLine(DateTime.UtcNow);
                    Console.WriteLine(body);
                    context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
                    await next.Invoke();
                    context.Request.Body = initialBody;
                }
            });


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
