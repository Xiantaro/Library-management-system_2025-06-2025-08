using System.ComponentModel.DataAnnotations;

namespace test2.Areas.Frontend.Models
{
    public class Guest
    {
        [Required(ErrorMessage = "請輸入帳號")]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}