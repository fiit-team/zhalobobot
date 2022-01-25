using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Zhalobobot.Api.Server.Repositories.Feedback;
using Zhalobobot.Api.Server.Repositories.FiitStudentsData;
using Zhalobobot.Api.Server.Repositories.Schedule;
using Zhalobobot.Api.Server.Repositories.Students;
using Zhalobobot.Api.Server.Repositories.Subjects;
using Zhalobobot.Common.Clients.Core;

namespace Zhalobobot.Api.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
        
        private static void ConfigureRepositories(IServiceCollection services)
        {
            services.AddSingleton<IFeedbackRepository, FeedbackRepository>();
            services.AddSingleton<ISubjectRepository, SubjectRepository>();
            services.AddSingleton<IScheduleRepository, ScheduleRepository>();
            services.AddSingleton<IStudentRepository, StudentRepository>();
            services.AddSingleton<IFiitStudentsDataRepository, FiitStudentsDataRepository>();
        }
    }
}