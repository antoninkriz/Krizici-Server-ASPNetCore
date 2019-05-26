using System;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using KriziciServer.Common.Auth;
using KriziciServer.Common.GoogleDrive;
using KriziciServer.Common.RabbitMq;
using KriziciServer.Common.Requests;
using KriziciServer.Common.Responses;
using KriziciServer.Services.Data.Handlers;
using KriziciServer.Services.Data.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KriziciServer.Services.Data
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddLogging();
            services.AddJwt(Configuration);
            services.AddRabbitMq(Configuration);
            services.AddGoogleDrive(Configuration);
            services.AddScoped<IDataService, DataService>();
            services.AddScoped<IRequestHandler<ImageRequest, ImageResponse>, RequestImageHandler>();
            services.AddScoped<IRequestHandler<DataRequest, DataResponse>, RequestDataHandler>();

            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}