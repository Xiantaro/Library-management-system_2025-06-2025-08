using System.ComponentModel.DataAnnotations;

namespace test2.Areas.Frontend.Models.Dtos
{
    public class UserRegistrationDto
    {
        [Required(ErrorMessage = "姓名不能為空")]
        [StringLength(30, ErrorMessage = "姓名長度不能超過50個字元")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "電話號碼不能為空")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "請輸入有效的行動電話號碼")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子信箱不能為空")]
        [EmailAddress(ErrorMessage = "電子信箱格式錯誤")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼不能為空")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "密碼長度不得小於4")]
        [MaxLength(20, ErrorMessage = "密碼長度不得大於20")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z0-9]*$",
                      ErrorMessage = "密碼強度不足，請包含大小寫字母、數字")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "請再次輸入密碼")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "兩次密碼輸入不同")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
