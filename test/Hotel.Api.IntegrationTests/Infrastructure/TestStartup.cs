using Hotel.Api.Infrastructure.Filters;
using Hotel.Api.IntegrationTests.Infrastructure.DataFeeders;
using Hotel.Core;
using Hotel.Core.Extensions;
using Hotel.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Hotel.Api.IntegrationTests.Infrastructure
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor()
                .AddMvcCore(options => { options.Filters.Add<ValidateModelStateFilter>(); })
                .AddDataAnnotations();

            services.AddCoreComponents();
            // services.AddTransient<ISomeService, SomeService>();  //if needed override registration with own test fakes
            services.AddFeatureManagement();
            services.AddDbContext<RoomsContext>(options =>
                options.UseInMemoryDatabase("hotel")
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var roomsContext = app.ApplicationServices.GetService<RoomsContext>();
            RoomsDataFeeder.Feed(roomsContext);

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
