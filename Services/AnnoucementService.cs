// Services/AnnouncementService.cs

using Microsoft.EntityFrameworkCore; // 引入 EF Core 命名空間
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using test2.Areas.Frontend.Models.ViewModels;
using test2.Models;

namespace test2.Services
{
    public class AnnouncementService
    {
        private readonly Test2Context _dbContext; // 注入 DbContext
        private readonly ActivityService _activityService;

        public AnnouncementService(Test2Context dbcontext, ActivityService activityService)
        {
            _dbContext = dbcontext;
            _activityService = activityService;
        }

        /// <summary>
        /// 獲取指定數量的公告及其類別，若不傳入或傳入0，則獲取全部。
        /// </summary>
        /// <param name="count">要獲取的公告數量</param>
        /// <returns>公告列表。</returns>
        public async Task<List<Announcement>> GetAnnouncementsAsync(int? count = 0)
        {
            var query = _dbContext.Announcements
                                    .Include(a => a.AnnouncementType)
                                    .OrderByDescending(a => a.Date);

            if (count.HasValue && count.Value > 0)
            {
                query.Take(count.Value);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// 獲取所有公告類別。
        /// </summary>
        /// <returns>公告類別列表。</returns>
        public async Task<List<AnnouncementType>> GetAllAnnouncementTypesAsync()
        {
            return await _dbContext.AnnouncementTypes
                                   .OrderBy(at => at.AnnouncementType1)
                                   .ToListAsync();
        }

        /// <summary>
        /// 使用 EF Core 獲取分頁過後的公告列表，可選公告類別和標題篩選。
        /// </summary>
        /// <param name="pageNumber">當前頁碼。</param>
        /// <param name="pageSize">每頁顯示筆數。</param>
        /// <param name="AnnouncementTypeName">要顯示的公告名稱，可選參數，預設為 null。</param>
        /// <param name="searchQuery">要搜尋的活動標題，可選參數，預設為 null。</param>
        /// <returns>包含分頁公告資料的 HomeIndexViewModel。</returns>
        public async Task<HomeIndexViewModel> GetPagedAnnouncementsAsync(
            int pageNumber,
            int pageSize,
            string AnnouncementTypeName = "",
            string? searchQuery = null)
        {
            // 確保頁碼和每頁筆數至少為 1
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, pageSize);

            // 初始化查詢
            var query = _dbContext.Announcements.AsQueryable();

            // 如果有指定活動類別，就加上篩選條件
            if (!string.IsNullOrEmpty(AnnouncementTypeName))
            {
                query = query.Where(a => a.AnnouncementType != null && a.AnnouncementType.AnnouncementType1 == AnnouncementTypeName);
            }

            // 如果有指定搜尋關鍵字，就加上標題篩選條件
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(a => a.AnnouncementTitle != null && a.AnnouncementTitle.ToLower().Contains(searchQuery.ToLower()));
            }

            // 計算總筆數
            var totalCount = await query.CountAsync();

            // 計算總頁數
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 獲取當前頁的公告資料
            var announcements = await query
                                     .OrderByDescending(a => a.Date) // 排序
                                     .Include(a => a.AnnouncementType) // 載入 AnnouncementType
                                     .Skip((pageNumber - 1) * pageSize) // 跳過前面頁的資料
                                     .Take(pageSize) // 取出當前頁的資料
                                     .ToListAsync();

            var announcementsTypes = await _dbContext.AnnouncementTypes
                                       .OrderBy(at => at.AnnouncementType1)
                                       .ToListAsync();


            // 將結果封裝到 HomeIndexViewModel
            var viewModel = new HomeIndexViewModel()
            {
                Announcements = announcements,
                AnnouncementTypes = announcementsTypes,
                CurrentPage = pageNumber,
                CurrentType = AnnouncementTypeName,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
            };

            return viewModel;
        }
    }
}