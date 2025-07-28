using System.Collections.Generic;
using test2.Areas.Frontend.Models.Dtos;
using test2.Models;

namespace test2.Areas.Frontend.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Announcement> Announcements { get; set; } = new List<Announcement>();
        public List<AnnouncementType> AnnouncementTypes { get; set; } = new List<AnnouncementType>();
        public List<ActivityViewModel> Activities { get; set; } = new List<ActivityViewModel>();

        /// <summary>
        /// 總頁數。
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 總公告筆數。
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 目前的頁碼。
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 目前顯示的公告種類。
        /// </summary>
        public string? CurrentType { get; set; }

        /// <summary>
        /// 每頁顯示的公告筆數。
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 最新書籍
        /// </summary>
        public List<Book> NewBooks { get; set; } = new List<Book>();

        /// <summary>
        /// 熱門書籍
        /// </summary>
        public List<Book> PopularBooks { get; set; } = new List<Book>();
    }
}
