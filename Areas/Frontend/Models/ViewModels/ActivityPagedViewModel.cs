// test2.Areas.Frontend.Models.ViewModels/ActivityListViewModel.cs
using System.Collections.Generic;

namespace test2.Areas.Frontend.Models.ViewModels
{
    /// <summary>
    /// 活動列表頁的 ViewModel，包含活動列表和分頁資訊。
    /// </summary>
    public class ActivityPagedViewModel
    {
        /// <summary>
        /// 活動的 ViewModel 列表。
        /// </summary>
        public List<ActivityViewModel> Activities { get; set; } = new List<ActivityViewModel>();

        /// <summary>
        /// 目前的頁碼。
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁顯示的筆數。
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 總頁數。
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 總活動筆數。
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 顯示模式 (例如: "image" 或 "table")。
        /// </summary>
        public string DisplayMode { get; set; } = "image";
    }
}