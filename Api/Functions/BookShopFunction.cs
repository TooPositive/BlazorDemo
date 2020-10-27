using BlazorApp.Api.Models;
using BlazorApp.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "booksHub")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }


        [FunctionName("AllBooks")]
        public async Task<IActionResult> GetBooks(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
                ILogger log)
        {
            try
            {
                List<Shared.Models.Book> books = await GetAllBooksWithImages();
                return new OkObjectResult(books);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [FunctionName("RefreshBooks")]
        public async Task<IActionResult> PostRefreshBooks(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
                [SignalR(HubName = "booksHub")] IAsyncCollector<SignalRMessage> signalRMessages,
                ILogger log)
        {
            try
            {
                var books = await _booksDBContext.Books.ToListAsync();
                books.ForEach(x => x.IsBought = false);
                await _booksDBContext.SaveChangesAsync();
                string booksJson = await GetRefreshedBooksAsSingalRMessage(signalRMessages);
                return new OkObjectResult(booksJson);
            }
            catch (Exception)
            {
                return new BadRequestObjectResult("Cannot mark all books as not bought. Check logs.");
            }

        }

        [FunctionName("BuyBook")]
        public async Task<IActionResult> PostBuyBook(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
                [SignalR(HubName = "booksHub")] IAsyncCollector<SignalRMessage> signalRMessages,
                ILogger log)
        {
            try
            {
                var bodyContent = await new StreamReader(req.Body).ReadToEndAsync();
                var boughtBook = JsonConvert.DeserializeObject<Shared.Models.Book>(bodyContent);
                var bookToUpdate = await _booksDBContext.Books.SingleOrDefaultAsync(x => x.Id == boughtBook.Id);
                bookToUpdate.IsBought = true;
                await _booksDBContext.SaveChangesAsync();
                string booksJson = await GetRefreshedBooksAsSingalRMessage(signalRMessages);
                return new OkObjectResult(booksJson);
            }
            catch (Exception)
            {
                return new BadRequestObjectResult("Cannot mark book as bought. Check logs.");
            }
        }

        private async Task UpdateBlobImages(IEnumerable<Book> books)
        {
            var storageCredentials = new StorageCredentials(Statics.AzureStorageAccount, Statics.AzureStorageKey);
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, Statics.AzureStorageAccount, null, true);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(Statics.BlobContainerName);

            foreach (var book in books)
            {
                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(book.BlobImageName);
                var ms = new MemoryStream();
                await cloudBlockBlob.DownloadToStreamAsync(ms);
                book.BlobImage = ms.ToArray();
            }
        }

        private async Task<string> GetRefreshedBooksAsSingalRMessage(IAsyncCollector<SignalRMessage> signalRMessages)
        {
            List<Shared.Models.Book> books = await GetAllBooksWithImages();
            string booksJson = JsonConvert.SerializeObject(books);
            await signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "booksRefreshed",
                Arguments = new[] { booksJson }
            });
            return booksJson;
        }

        private async Task<List<Shared.Models.Book>> GetAllBooksWithImages()
        {
            var books = await _booksDBContext.Books.ToListAsync();
            await UpdateBlobImages(books);
            return books;
        }
    }
}