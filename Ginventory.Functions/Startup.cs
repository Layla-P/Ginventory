using System;
using System.Linq;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ginventory.Functions;
using Ginventory.Functions.Data;
using Microsoft.EntityFrameworkCore;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Ginventory.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // ------------------ default configuration initialise ------------------
            var serviceConfig = builder
                                .Services
                                .FirstOrDefault(s => s.ServiceType == typeof(IConfiguration));
            
            if (serviceConfig != null)
            {
                _ = (IConfiguration)serviceConfig.ImplementationInstance;
            }

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Environment.GetEnvironmentVariable("DefaultConnection")));
            
            builder.Services.AddLogging();
            builder.Services.AddDbContext<ApplicationDbContext>();
            
        }
    }
}