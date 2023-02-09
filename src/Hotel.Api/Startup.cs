using FastExpressionCompiler;
using HealthChecks.UI.Client;
using Hotel.Api.Infrastructure.Filters;
using Hotel.Api.Infrastructure.Registrations;
using Hotel.Core.Extensions;
using Hotel.Core.Settings;
using Hotel.Db;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Hotel.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
            services
                .AddHttpContextAccessor()
                .AddRouting(options => options.LowercaseUrls = true)
                .AddMvcCore(options =>
                {
                    options.Filters.Add<HttpGlobalExceptionFilter>();
                    options.Filters.Add<ValidateModelStateFilter>();
                    options.Filters.Add<ApiKeyAuthorizationFilter>();
                })
                .AddApiExplorer()
                .AddDataAnnotations();

            //there is a difference between AddDbContext() and AddDbContextPool(), more info https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-2.0#dbcontext-pooling and https://stackoverflow.com/questions/48443567/adddbcontext-or-adddbcontextpool
            //services.AddDbContext<EmployeesContext>(options => options.UseMySql(_configuration.GetConnectionString("MySqlDb"), ServerVersion.Parse("8")), contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
            services.AddDbContextPool<RoomsContext>(
                options => options.UseSqlServer(_configuration.GetConnectionString("MsSqlDb")), poolSize: 10);

            services.Configure<ApiKeySettings>(_configuration.GetSection("ApiKey"));
            services.AddSwagger(_configuration);

            //services.Configure<PingWebsiteSettings>(_configuration.GetSection("PingWebsite"));
            //services.AddHostedService<PingWebsiteBackgroundService>();
            //services.AddHttpClient(nameof(PingWebsiteBackgroundService));

            services.AddCoreComponents();

            services.AddFeatureManagement()
                .AddFeatureFilter<TimeWindowFilter>();

            services.AddHealthChecks()
                .AddSqlServer(_configuration.GetConnectionString("MsSqlDb"));
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple Api V1");
                c.DocExpansion(DocExpansion.None);
            });
        }
    }
}
