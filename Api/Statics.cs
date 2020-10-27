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
        public static string AzureStorageAccount = Environment.GetEnvironmentVariable("AzureStorageAccount");
        public static string AzureStorageKey = Environment.GetEnvironmentVariable("AzureStorageKey");
        public static string BlobContainerName = "blazordemo";
    }
}
