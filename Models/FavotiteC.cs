using System.ComponentModel.DataAnnotations;

namespace test2.Models
{
    public class FavotiteC
    {
        [Key]
        public string cId { get; set; } = string.Empty;
        public string collectionId { get; set; } = string.Empty;
    }
}