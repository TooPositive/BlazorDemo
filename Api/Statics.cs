using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Api
{
    public static class Statics
    {
        public static string ConnectionString = Environment.GetEnvironmentVariable("MyConnectionString");
    }
}
