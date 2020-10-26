using BlazorApp.Api.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(BlazorApp.Api.Startup))]

namespace BlazorApp.Api
{    
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {            
            builder.Services.AddDbContext<BookDbContext>(
                options => options.UseSqlServer(Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process)));
        }
    }
}
