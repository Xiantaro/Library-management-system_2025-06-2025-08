// Services/ActivityService.cs

using Microsoft.EntityFrameworkCore; // 引入 EF Core 命名空間
using System.Collections.Generic;
using System.Linq; // 用於 LINQ 查詢

using test2.Models; // 引入 DbContext 命名空間
using test2.Areas.Frontend.Models.ViewModels;

namespace test2.Services
{
    public class ActivityService
    {
        private readonly Test2Context _dbContext; // <--- 注入 DbContext

        public ActivityService(Test2Context dbContext) // <--- 建構子注入 DbContext
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 使用 EF Core 將新的活動資料存入資料庫。
        /// </summary>
        public async Task<int> InsertNewActivity(
            string title,
            string description,
            byte[] imageData,
            DateTime start,
            DateTime end,
            int cap,
            int typeId,
            int audienceId)
        {
            // 建立 Entity 物件，並加入 DbSet
            var newActivity = new Activity
            {
                ActivityTitle = title,
                ActivityDesc = description,
                ActivityImg = imageData,
                StartDate = start,
                EndDate = end,
                Capacity = cap,
                ActivityTypeId = typeId,
                AudienceId = audienceId
            };

            _dbContext.Activities.Add(newActivity); // 將物件加入到 DbSet 中
            await _dbContext.SaveChangesAsync(); // <--- 儲存變更到資料庫

            return newActivity.ActivityId; // EF Core 會在 SaveChangesAsync() 後自動填入 IDENTITY ID
        }

        /// <summary>
        /// 使用 EF Core 從資料庫獲取所有活動的列表。
        /// </summary>
        public async Task<List<ActivityViewModel>> GetAllActivitiesAsync()
        {
            // <--- 使用 LINQ 查詢資料庫，並將結果投射 (Project) 到 ViewModel
            var activities = await _dbContext.Activities
                                             .OrderByDescending(a => a.ActivityId) // 排序
                                             .Include(a => a.Audience)
                                             .Include(a => a.ActivityType)
                                             .Select(a => new ActivityViewModel // 投射到 ViewModel
                                             {
                                                 ActivityId = a.ActivityId,
                                                 ActivityTitle = a.ActivityTitle,
                                                 ActivityDesc = a.ActivityDesc,
                                                 // 圖片轉換 Base64，EF Core 不直接處理，需要在 C# 轉換
                                                 ActivityImgBase64 = a.ActivityImg != null ? Convert.ToBase64String(a.ActivityImg) : null,
                                                 StartDate = a.StartDate,
                                                 EndDate = a.EndDate,
                                                 Capacity = a.Capacity,
                                                 ActivityTypeId = a.ActivityTypeId,
                                                 AudienceId = a.AudienceId,
                                                 Audience = a.Audience,
                                                 ActivityType = a.ActivityType
                                             })
                                             .ToListAsync(); // 執行查詢並獲取列表

            return activities;
        }

        /// <summary>
        /// 使用 EF Core 從資料庫獲取指定數量的活動的列表，若不指定數量預設為三筆。
        /// </summary>
        public async Task<List<ActivityViewModel>> GetActivitiesAsync(int count = 3)
        {
            // <--- 使用 LINQ 查詢資料庫，並將結果投射 (Project) 到 ViewModel
            var activities = await _dbContext.Activities
                                             .OrderBy(a => a.ActivityId) // 排序
                                             .Include(a => a.Audience)
                                             .Include(a => a.ActivityType)
                                             .Take(count)
                                             .Select(a => new ActivityViewModel // 投射到 ViewModel
                                             {
                                                 ActivityId = a.ActivityId,
                                                 ActivityTitle = a.ActivityTitle,
                                                 ActivityDesc = a.ActivityDesc,
                                                 // 圖片轉換 Base64，EF Core 不直接處理，需要在 C# 轉換
                                                 ActivityImgBase64 = a.ActivityImg != null ? Convert.ToBase64String(a.ActivityImg) : null,
                                                 StartDate = a.StartDate,
                                                 EndDate = a.EndDate,
                                                 Capacity = a.Capacity,
                                                 ActivityTypeId = a.ActivityTypeId,
                                                 AudienceId = a.AudienceId,
                                                 Audience = a.Audience,
                                                 ActivityType = a.ActivityType
                                             })
                                             .ToListAsync(); // 執行查詢並獲取列表

            return activities;
        }

        /// <summary>
        /// 根據活動標題從資料庫獲取單一活動的詳細資訊。
        /// </summary>
        /// <param name="activityTitle">活動標題。</param>
        /// <returns>對應的 ActivityViewModel，如果找不到則為 null。</returns>
        public async Task<ActivityViewModel?> GetActivityByTitleAsync(string activityTitle)
        {
            var activity = await _dbContext.Activities
                                           .Include(a => a.Audience) // 如果需要載入 Audience 資訊
                                           .Include(a => a.ActivityType)
                                           .Where(a => a.ActivityTitle == activityTitle) // 根據標題篩選
                                           .Select(a => new ActivityViewModel
                                           {
                                               ActivityId = a.ActivityId,
                                               ActivityTitle = a.ActivityTitle,
                                               ActivityDesc = a.ActivityDesc,
                                               ActivityImgBase64 = a.ActivityImg != null ? Convert.ToBase64String(a.ActivityImg) : null,
                                               StartDate = a.StartDate,
                                               EndDate = a.EndDate,
                                               Capacity = a.Capacity,
                                               ActivityTypeId = a.ActivityTypeId,
                                               AudienceId = a.AudienceId,
                                               Audience = a.Audience, // 同樣確認 ViewModel 中 Audience 屬性型別
                                               ActivityType = a.ActivityType,
                                           })
                                           .SingleOrDefaultAsync(); // 使用 SingleOrDefaultAsync 獲取單一結果或 null

            return activity;
        }

        /// <summary>
        /// 使用 EF Core 獲取分頁過後的活動列表，可選活動類別和標題篩選。
        /// </summary>
        /// <param name="pageNumber">當前頁碼。</param>
        /// <param name="pageSize">每頁顯示筆數。</param>
        /// <param name="displayMode">顯示模式 (例如："image"、"table")。</param>
        /// <param name="activityTypeName">活動類別的名稱 (例如："活動"、"講座")，可選參數，預設為 null。</param>
        /// <param name="searchQuery">要搜尋的活動標題，可選參數，預設為 null。</param>
        /// <returns>包含分頁活動資料的 ActivityPagedViewModel。</returns>
        public async Task<ActivityPagedViewModel> GetPagedActivitiesAsync(
            int pageNumber,
            int pageSize,
            string displayMode,
            string? activityTypeName = null,
            string? searchQuery = null)
        {
            // 確保頁碼和每頁筆數至少為 1
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, pageSize);

            // 初始化查詢
            var query = _dbContext.Activities.AsQueryable();

            // 如果有指定活動類別，就加上篩選條件
            if (!string.IsNullOrEmpty(activityTypeName))
            {
                query = query.Where(a => a.ActivityType != null && a.ActivityType.ActivityType1 == activityTypeName);
            }

            // 如果有指定搜尋關鍵字，就加上標題篩選條件
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(a => a.ActivityTitle != null && a.ActivityTitle.ToLower().Contains(searchQuery.ToLower()));
            }

            // 計算總筆數
            var totalCount = await query.CountAsync();

            // 計算總頁數
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 獲取當前頁的活動資料
            var activities = await query
                                     .OrderByDescending(a => a.ActivityId) // 排序
                                     .Include(a => a.Audience) // 載入 Audience
                                     .Include(a => a.ActivityType) // 載入 ActivityType
                                     .Skip((pageNumber - 1) * pageSize) // 跳過前面頁的資料
                                     .Take(pageSize) // 取出當前頁的資料
                                     .Select(a => new ActivityViewModel
                                     {
                                         ActivityId = a.ActivityId,
                                         ActivityTitle = a.ActivityTitle,
                                         ActivityDesc = a.ActivityDesc,
                                         ActivityImgBase64 = a.ActivityImg != null ? Convert.ToBase64String(a.ActivityImg) : null,
                                         StartDate = a.StartDate,
                                         EndDate = a.EndDate,
                                         Capacity = a.Capacity,
                                         ActivityTypeId = a.ActivityTypeId,
                                         AudienceId = a.AudienceId,
                                         Audience = a.Audience,
                                         ActivityType = a.ActivityType,
                                     })
                                     .ToListAsync();

            // 將結果封裝到 ActivityPagedViewModel
            var viewModel = new ActivityPagedViewModel
            {
                Activities = activities,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
                DisplayMode = displayMode
            };

            return viewModel;
        }

    }
}