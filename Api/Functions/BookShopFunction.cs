using BlazorApp.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Api
{
    public class BookShopFunction
    {
        private readonly BookDbContext _booksDBContext;


        public BookShopFunction(BookDbContext booksDBContext)
        {
            _booksDBContext = booksDBContext;
        }

        [FunctionName("AllBooks")]
        public async Task<IActionResult> GetBooks(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
                ILogger log)
        {

            var books = await _booksDBContext.Books.ToListAsync();
            return new OkObjectResult(books);
        }

        [FunctionName("RefreshBooks")]
        public async Task<IActionResult> PostRefreshBooks(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
                ILogger log)
        {
            try
            {
                var books = await _booksDBContext.Books.ToListAsync();
                books.ForEach(x => x.IsBought = false);
                await _booksDBContext.SaveChangesAsync();
                return new OkObjectResult("All books are mark as not bought.");
            }
            catch (Exception)
            {
                return new BadRequestObjectResult("Cannot mark all books as not bought. Check logs.");
            }

        }
    }
}
