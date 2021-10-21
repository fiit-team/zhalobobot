using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Zhalobobot.Api.Server.Repositories;
using Zhalobobot.Common.Clients.Core;

namespace Zhalobobot.Api.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private Settings Settings { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Settings = configuration.GetSection("Settings").Get<Settings>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Zhalobobot.Api.Server", Version = "v1" });
            });
            
            services.AddSingleton(Settings);
            services.AddSingleton<IZhalobobotApiClient, ZhalobobotApiClient>();

            ConfigureRepositories(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zhalobobot.Api.Server v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
        
        private void ConfigureRepositories(IServiceCollection services)
        {
            var scopes = new[] { SheetsService.Scope.Spreadsheets };

            GoogleCredential credential;

            using (var stream = new FileStream(Settings.CredentialsFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(scopes);
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Settings.ApplicationName,
            });

            services.AddSingleton(service.Spreadsheets);
            services.AddSingleton<IGoogleSheetsRepository, GoogleSheetsRepository>();
        }
    }
}