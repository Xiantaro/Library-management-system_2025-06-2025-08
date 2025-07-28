using System;
using System.ComponentModel.DataAnnotations; // 用於資料驗證屬性，雖然這裡主要用於顯示
using test2.Models;

namespace test2.Areas.Frontend.Models.ViewModels
{
    public class ActivityViewModel
    {
        [Display(Name = "活動 ID")] // 這是顯示在網頁上的標籤名稱
        public int ActivityId { get; set; }

        [Display(Name = "活動標題")]
        public string? ActivityTitle { get; set; }

        [Display(Name = "活動描述")]
        public string? ActivityDesc { get; set; }

        // 圖片通常會以 base64 字串或圖片 URL 的形式傳遞給 View 顯示
        // 這裡圖片是轉換成 Base64 字串來顯示
        [Display(Name = "活動圖片")]
        public string? ActivityImgBase64 { get; set; } // 用於在 View 顯示圖片

        [Display(Name = "開始日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "結束日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Display(Name = "人數上限")]
        public int Capacity { get; set; }

        [Display(Name = "活動地點")]
        public string ActivityLocation { get; set; } = string.Empty;

        [Display(Name = "活動類別 ID")]
        public int ActivityTypeId { get; set; }

        [Display(Name = "受眾種類 ID")]
        public int AudienceId { get; set; }

        [Display(Name = "適用對象")]
        public Audience? Audience { get; set; }

        [Display(Name = "活動種類")]
        public ActivityType? ActivityType { get; set; }
    }
}