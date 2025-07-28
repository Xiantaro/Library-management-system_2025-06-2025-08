using System.ComponentModel.DataAnnotations;

namespace test2.Models
{
    public class CommentC
    {
        [Key]
        public int score { get; set; }
        public string feedback { get; set; } = string.Empty;
    }
}