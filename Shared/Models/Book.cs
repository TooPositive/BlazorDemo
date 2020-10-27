using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.Shared.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Author  { get; set; }
        public string Title  { get; set; }
        public double Price  { get; set; }
        public bool IsBought { get; set; }        
        public string BlobImageName { get; set; }

        [NotMapped]
        public byte[] BlobImage { get; set; }
    }
}