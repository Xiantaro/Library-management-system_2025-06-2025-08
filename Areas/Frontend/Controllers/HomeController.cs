using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using test2.Models;
using test2.Controllers;
using test2.Services;
using test2.Areas.Frontend.Models;
using test2.Areas.Frontend.Models.Dtos;
using test2.Areas.Frontend.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace test2.Areas.Frontend.Controllers
{
    [Area("Frontend")]
    public class HomeController : UserController
    {
        #region field
        public const string sk1 = "query1";
        public const string sk2 = "type1";
        public const string sk3 = "query2";
        public const string sk4 = "year1";
        public const string sk5 = "year2";
        public const string sk6 = "lang";
        public const string sk7 = "type2";
        public const string sk8 = "status";

        private readonly Test2Context _context;

        private readonly ILogger<HomeController> _logger;
        private readonly ActivityService _activityService;
        private readonly AnnouncementService _announcementService;
        private readonly UserService _userService;
        #endregion

        #region constructor
        public HomeController(ILogger<HomeController> logger, Test2Context context, ActivityService activityService, AnnouncementService announcementService, UserService userService)
        {
            _logger = logger;
            _activityService = activityService;
            _announcementService = announcementService;
            _userService = userService;
            _context = context;
        }
        #endregion

        #region action
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Query(string query1, string type1, string query2, string year1, string year2, string lang, string type2, string status)
        {
            if (string.IsNullOrEmpty(query1)) { query1 = string.Empty; }
            if (string.IsNullOrEmpty(type1)) { type1 = string.Empty; }
            if (string.IsNullOrEmpty(query2)) { query2 = string.Empty; }
            if (string.IsNullOrEmpty(year1)) { year1 = string.Empty; }
            if (string.IsNullOrEmpty(year2)) { year2 = string.Empty; }
            if (string.IsNullOrEmpty(lang)) { lang = string.Empty; }
            if (string.IsNullOrEmpty(type2)) { type2 = string.Empty; }
            if (string.IsNullOrEmpty(status)) { status = string.Empty; }

            HttpContext.Session.SetString(sk1, query1);
            HttpContext.Session.SetString(sk2, type1);
            HttpContext.Session.SetString(sk3, query2);
            HttpContext.Session.SetString(sk4, year1);
            HttpContext.Session.SetString(sk5, year2);
            HttpContext.Session.SetString(sk6, lang);
            HttpContext.Session.SetString(sk7, type2);
            HttpContext.Session.SetString(sk8, status);

            IQueryable<Collection> query = _context.Collections.Include(x => x.Author).Include(x => x.Language).Include(x => x.Type).Include(x => x.Books);

            if (!string.IsNullOrEmpty(query1)) { query = query.Where(i => i.Title.Contains(query1) || i.Author.Author1.Contains(query1) || i.Publisher.Contains(query1) || i.Isbn.Contains(query1)); }
            else
            {
                bool x = int.TryParse(year1, out int yearX);
                bool y = int.TryParse(year2, out int yearY);

                if (!string.IsNullOrEmpty(query2))
                {
                    switch (type1)
                    {
                        case "title":
                            query = query.Where(i => i.Title.Contains(query2)); break;
                        case "author":
                            query = query.Where(i => i.Author.Author1.Contains(query2)); break;
                        case "publisher":
                            query = query.Where(i => i.Publisher.Contains(query2)); break;
                        case "isbn":
                            query = query.Where(i => i.Isbn.Contains(query2)); break;
                        default: break;
                    }
                }
                if (x) { query = query.Where(i => i.PublishDate.Year >= yearX); }
                if (y) { query = query.Where(i => i.PublishDate.Year <= yearY); }
                if (!string.IsNullOrEmpty(lang)) { query = query.Where(i => i.Language.LanguageId == Convert.ToInt16(lang)); }
                if (!string.IsNullOrEmpty(type2)) { query = query.Where(i => i.Type.TypeId == Convert.ToInt16(type2)); }
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Collection(string type, string author, string publisher, string lang, string year1, string year2)
        {
            IQueryable<Collection> collection = _context.Collections.Include(x => x.Author).Include(x => x.Language).Include(x => x.Type).Include(x => x.Books);

            bool x = int.TryParse(year1, out int yearX);
            bool y = int.TryParse(year2, out int yearY);

            if (!string.IsNullOrEmpty(type)) { collection = collection.Where(i => i.Type.Type1.Contains(type)); }
            if (!string.IsNullOrEmpty(author)) { collection = collection.Where(i => i.Author.Author1.Contains(author)); }
            if (!string.IsNullOrEmpty(publisher)) { collection = collection.Where(i => i.Publisher.Contains(publisher)); }
            if (!string.IsNullOrEmpty(lang)) { collection = collection.Where(i => i.Language.Language1.Contains(lang)); }
            if (x) { collection = collection.Where(i => i.PublishDate.Year >= yearX); }
            if (y) { collection = collection.Where(i => i.PublishDate.Year <= yearY); }

            return View(await collection.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReserve(string user, string collection, string status)
        {
            int.TryParse(user, out int cId);
            int.TryParse(collection, out int collectionId);

            bool reservationStatus = await _context.Reservations.AnyAsync(x => x.CId == cId && x.CollectionId == collectionId && x.ReservationStatusId == 2);

            if (reservationStatus) { return RedirectToAction(status == "C" ? "Collection" : "Query"); }

            var reservationDate = await _context.Reservations.Where(x => x.CId == cId && x.CollectionId == collectionId).OrderByDescending(x => x.ReservationDate).FirstOrDefaultAsync();

            if (reservationDate != null)
            {
                TimeSpan reservationDateDiff = DateTime.Now - reservationDate.ReservationDate;

                if (reservationDateDiff.TotalDays < 1) { return RedirectToAction(status == "C" ? "Collection" : "Query"); }
            }

            var reservation = new Reservation
            {
                CId = cId,
                CollectionId = collectionId,
                ReservationDate = DateTime.Now,
                ReservationStatusId = 2
            };

            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            return RedirectToAction(status == "C" ? "Collection" : "Query");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReserve(string user, string collection)
        {
            int.TryParse(user, out int cId);
            int.TryParse(collection, out int collectionId);

            var reservationId = await _context.Reservations.FirstOrDefaultAsync(x => x.CId == cId && x.CollectionId == collectionId && x.ReservationStatusId == 2);

            reservationId!.ReservationStatusId = 4;

            await _context.SaveChangesAsync();

            return RedirectToAction("Client");
        }

        [HttpGet]
        public async Task<IActionResult> Client()
        {
            var userNameX = Convert.ToString(ViewData["Name"]);

            IQueryable<Client> client = _context.Clients.Include(x => x.Reservations).ThenInclude(y => y.ReservationStatus)
                                                                                     .Include(x => x.Reservations).ThenInclude(y => y.Collection).ThenInclude(z => z.Author)
                                                                                     .Include(x => x.Borrows).ThenInclude(y => y.BorrowStatus)
                                                                                     .Include(w => w.Borrows).ThenInclude(x => x.Book).ThenInclude(y => y.Collection).ThenInclude(z => z.Author)
                                                                                     .Include(x => x.Borrows).ThenInclude(y => y.Histories)
                                                                                     .Include(x => x.Favorites).ThenInclude(y => y.Collection).ThenInclude(z => z.Author)
                                                                                     .Include(x => x.Notifications)
                                                                                     .Where(x => x.CName.Contains(userNameX!));

            return View(await client.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(string idInput1, string passwordInput3)
        {
            int.TryParse(idInput1, out int cId);

            var userId = await _context.Clients.FindAsync(cId);

            userId!.CPassword = Argon2.Hash(passwordInput3);

            await _context.SaveChangesAsync();

            return RedirectToAction("Client");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhone(string idInput2, string phoneInput2)
        {
            int.TryParse(idInput2, out int cId);

            //await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC pr_updatePhone {cId}, {phoneInput2}");

            //var userPhoneX = await _context.Clients.Include(x => x.Borrows).ThenInclude(y => y.BorrowStatus)
            //                                                                  .Include(w => w.Borrows).ThenInclude(x => x.Book).ThenInclude(y => y.Collection).ThenInclude(z => z.Author)
            //                                                                  .Include(x => x.Borrows).ThenInclude(y => y.Histories)
            //                                                                  .Include(x => x.Favorites).ThenInclude(y => y.Collection).ThenInclude(z => z.Author)
            //                                                                  .Include(x => x.Notifications)
            //                                                                  .AsNoTracking().FirstOrDefaultAsync(i => i.CId == cId);

            //return RedirectToAction("Client", new List<Client> { userPhoneX! });

            var userId = await _context.Clients.FindAsync(cId);

            userId!.CPhone = phoneInput2;

            await _context.SaveChangesAsync();

            return RedirectToAction("Client");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFavorite(string idInput3, string collectionIdInput1)
        {
            int.TryParse(idInput3, out int cId);
            int.TryParse(collectionIdInput1, out int collectionId);

            var favorites = new Favorite
            {
                CId = cId,
                CollectionId = collectionId
            };

            _context.Favorites.Add(favorites!);

            await _context.SaveChangesAsync();

            return RedirectToAction("Client");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFavorite(string idInput4, string collectionIdInput2)
        {
            int.TryParse(idInput4, out int cId);
            int.TryParse(collectionIdInput2, out int collectionId);

            var favoritesId = await _context.Favorites.FirstOrDefaultAsync(x => x.CId == cId && x.CollectionId == collectionId);

            _context.Favorites.Remove(favoritesId!);

            await _context.SaveChangesAsync();

            return RedirectToAction("Client");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment(string borrowIdInput, string rate, string comment)
        {
            int.TryParse(borrowIdInput, out int borrowId);
            int.TryParse(rate, out int score);

            var borrowIdX = await _context.Histories.FindAsync(borrowId);

            borrowIdX!.Score = score;
            borrowIdX!.Feedback = comment;

            await _context.SaveChangesAsync();

            return RedirectToAction("Client");
        }

        #region 有修改的部分

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string displayType = "", string searchQuery = "")
        {
            var viewModel = new HomeIndexViewModel();

            try
            {
                // 從 AnnouncementService 獲取公告資料
                viewModel = await _announcementService.GetPagedAnnouncementsAsync(pageNumber, pageSize, displayType, searchQuery);
                // 從 ActivityService 獲取公告資料
                viewModel.Activities = await _activityService.GetActivitiesAsync();

                // 查詢最新書籍 (分兩階段處理以避免翻譯錯誤)
                // 第一階段：先從資料庫取出所有書籍及其 Collection 資訊
                var allBooksWithCollection = await _context.Books
                                                         .Include(b => b.Collection)
                                                            .ThenInclude(c => c.Author) // 在 Collection 載入後，再載入 Author 資訊
                                                         .ToListAsync(); // 這裡會將所有相關書籍載入記憶體


                // 第二階段：在記憶體中進行分組、排序和篩選
                viewModel.NewBooks = allBooksWithCollection
                                         .GroupBy(b => b.Collection?.CollectionId) // 根據 CollectionId 分組
                                         .Where(g => g.Key.HasValue) // 過濾掉 CollectionId 為 null 的分組
                                         .Select(g => g.OrderByDescending(b => b.AccessionDate).First()) // 在每個 CollectionId 組中，取出 AccessionDate 最新的那本
                                         .OrderByDescending(b => b.Collection?.PublishDate) // 再依出版日期降冪排序
                                         .Take(6)
                                         .ToList(); // 轉成 List

                // 查詢熱門書籍 (依照借閱次數 BorrowCount 降冪排序，取前 4 本)    

                viewModel.PopularBooks = await _context.Books
                                                        .Include(b => b.Collection) // 導覽屬性載入 Collection
                                                            .ThenInclude(c => c.Author) // 在 Collection 載入後，再載入 Author 資訊
                                                        .GroupJoin(
                                                            _context.Borrows,
                                                            book => book.BookId, // Book 的主鍵是 BookId
                                                            borrow => borrow.BookId, // Borrow 的外鍵是 BookId
                                                            (book, borrows) => new
                                                            {
                                                                Book = book,
                                                                BorrowCount = borrows.Count()
                                                            })
                                                        .OrderByDescending(x => x.BorrowCount)
                                                        .Select(x => x.Book)
                                                        .Take(4)
                                                        .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入 Home Index 資料時發生錯誤。");
                ViewData["Error"] = $"無法載入首頁資料: {ex.Message}";
                // 即使出錯，ViewModel 還是會有空列表，避免 View 報錯
            }

            return View(viewModel);
        }

        #endregion

        [HttpPost]
        /// <summary>
        /// 透過 AJAX POST 請求取得公告列表的資料，並可根據頁碼、公告類別進行篩選。
        /// </summary>
        /// <param name="pageNumber">當前頁碼。</param>
        /// <param name="pageSize">每頁顯示公告數量</param>
        /// <param name="displayType">要篩選的公告類別Id。</param>
        /// <param name="searchQuery">要搜尋的活動標題</param>
        public async Task<IActionResult> UpdateAnnouncementList(
            [FromForm] int pageNumber,
            [FromForm] int pageSize,
            [FromForm] string displayType,
            [FromForm] string searchQuery)
        {
            try
            {

                // 確保 displayType 有預設值，避免 null 參考錯誤
                displayType ??= "";  // 預設全部類別

                HomeIndexViewModel viewModel;

                // 呼叫 GetPagedActivitiesAsync
                // 如果 searchQuery 是 null，searchQuery 就傳 null
                viewModel = await _announcementService.GetPagedAnnouncementsAsync(
                    pageNumber,
                    pageSize,
                    displayType,
                    searchQuery == "null" ? null : searchQuery);

                return PartialView("~/Areas/Frontend/Views/Shared/_Partial/_announcementList.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新活動列表時發生錯誤。 傳入參數: Page = {pageNumber}, pageSize = {pageSize}, displayType = {displayType}, searchQuery = {searchQuery ?? "null"}");
                return StatusCode(500, $"更新活動列表失敗，請稍後再試: {ex.Message}");
            }
        }

        /// <summary>
        /// 顯示活動列表頁，支援分頁和顯示模式切換。
        /// </summary>
        /// <param name="page">當前頁碼，預設為 1。</param>
        /// <param name="pageSize">每頁顯示筆數，預設為 4。</param>
        /// <param name="displayMode">顯示模式 ("image" 或 "table")，預設為 "image"。</param>
        public async Task<IActionResult> Activity(int page = 1, int pageSize = 4, string displayMode = "image") // 預設第一頁，每頁4筆，圖片式顯示
        {
            var viewModel = new ActivityPagedViewModel();

            try
            {
                viewModel = await _activityService.GetPagedActivitiesAsync(page, pageSize, displayMode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入 Home Activity 資料時發生錯誤。");
                ViewData["Error"] = $"無法載入活動列表頁資料: {ex.Message}";
                // 即使出錯，ViewModel 還是會有空列表，避免 View 報錯
            }

            return View(viewModel); // 將包含完整分頁資訊的 ViewModel 傳遞給 View
        }

        [HttpPost]
        /// <summary>
        /// 透過 AJAX POST 請求取得活動列表的資料，並可根據頁碼、顯示模式和活動類別進行篩選。
        /// </summary>
        /// <param name="page">當前頁碼。</param>
        /// <param name="displayMode">要切換到的顯示模式 ("image" 或 "table")。</param>
        /// <param name="displayType">要篩選的活動類別 ("全部"、"講座" 等)。</param>
        /// <param name="searchQuery">要搜尋的活動標題</param>
        public async Task<IActionResult> UpdateActivityList(
            [FromForm] int page,
            [FromForm] string displayMode,
            [FromForm] string displayType,
            [FromForm] string searchQuery)
        {
            try
            {

                // 確保 displayMode 和 displayType 有預設值，避免 null 參考錯誤
                displayMode ??= "image"; // 預設圖片模式
                displayType ??= "全部";  // 預設全部類別

                // 根據 displayMode 決定 pageSize
                int pageSize = displayMode switch
                {
                    "image" => 4,
                    "table" => 6,
                    _ => -1 // 如果 displayMode 不是預期的模式就把 pageSize 設為 -1
                };

                if (pageSize == -1) // 如果 pageSize 為無效值 直接返回錯誤
                {
                    return BadRequest("無效的顯示模式");
                }

                ActivityPagedViewModel viewModel;

                // 呼叫 GetPagedActivitiesAsync
                // 如果 displayType 是 "全部"，activityTypeName 就傳 null
                // 如果 searchQuery 是 null，searchQuery 就傳 null
                viewModel = await _activityService.GetPagedActivitiesAsync(
                    page,
                    pageSize,
                    displayMode,
                    displayType == "全部" ? null : displayType,
                    searchQuery == "null" ? null : searchQuery);

                string? partialViewPath = displayMode switch
                {
                    "image" => "~/Areas/Frontend/Views/Shared/_Partial/_activityList_image.cshtml",
                    "table" => "~/Areas/Frontend/Views/Shared/_Partial/_activityList_table.cshtml",
                    _ => null
                };

                // 如果 partialViewPath 不再預期內 表示 displayMode 不在預期內
                if (partialViewPath == null)
                {
                    return BadRequest("無效的顯示模式，無法載入對應的視圖");
                }

                return PartialView(partialViewPath, viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新活動列表時發生錯誤。 傳入參數: Page = {page}, displayType = {displayType}, displayMode = {displayMode}, searchQuery = {searchQuery ?? "null"}");
                return StatusCode(500, $"更新活動列表失敗，請稍後再試: {ex.Message}");
            }
        }

        public async Task<IActionResult> ActivityInfo(string activityTitle)
        {
            try
            {
                // 呼叫時，型別會自動推斷為 ActivityViewModel?
                var viewModel = await _activityService.GetActivityByTitleAsync(activityTitle);

                if (viewModel == null) // <-- 這裡就是編譯器強制你檢查 null 的地方
                {
                    _logger.LogWarning($"找不到標題為 '{activityTitle}' 的活動。");
                    return NotFound($"找不到要查詢的活動: {activityTitle}");
                }

                return View(viewModel); // 如果 viewModel 不為 null，就可以安全地傳遞給 View
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"載入 Home ActivityInfo 資料時發生錯誤，活動標題: {activityTitle}。");
                ViewData["Error"] = $"無法載入活動資訊頁面資料: {ex.Message}";
                return View(new ActivityViewModel()); // 即使出錯，回傳一個空的 ViewModel
            }
        }

        /// <summary>
        /// 處理活動報名請求。
        /// </summary>
        /// <param name="activityId">要報名的活動 ID。</param>
        /// <returns>JSON 格式的報名結果。</returns>
        [HttpPost]
        public async Task<IActionResult> RegisterActivity(int activityId)
        {
            // 判斷使用者是否未登入
            if (!(User.Identity != null && User.Identity.IsAuthenticated))
            {
                // 未登入：直接告知前端需要登入，並提供登入頁面的 URL
                return Json(new
                {
                    success = false,
                    message = "請先登入才能報名活動喔！",
                    redirectToLogin = Url.Action("LoginC", "Access", new { area = "Frontend" }) // 提供正確的登入頁面URL
                });
            }

            // 已登入，繼續報名邏輯
            var userIdClaim = User.FindFirstValue(InternalClaimTypes.IId);
            if (!int.TryParse(userIdClaim, out int clientId))
            {
                // 無法取得使用者 ID (理論上登入後不會發生，但做個防範)
                return Json(new { success = false, message = "無法取得您的會員資訊，請重新登入。" });
            }

            var activity = await _context.Activities.FirstOrDefaultAsync(a => a.ActivityId == activityId);
            if (activity == null)
            {
                return Json(new { success = false, message = "找不到該活動。" });
            }

            // 檢查是否在報名時間內
            if (DateTime.Now < activity.StartDate || DateTime.Now > activity.EndDate)
            {
                return Json(new { success = false, message = "報名時間已過或尚未開始。" });
            }

            // 檢查名額
            if (activity.Capacity <= 0 && activity.Capacity != -1) // -1 表示無需報名，不檢查名額
            {
                return Json(new { success = false, message = "名額已滿，無法報名！" });
            }

            // 檢查是否已重複報名
            var existingRegistration = await _context.Participations
                                                     .FirstOrDefaultAsync(ar => ar.CId == clientId && ar.ActivityId == activityId);
            if (existingRegistration != null)
            {
                return Json(new { success = false, message = "您已經報名過此活動了！" });
            }

            // 執行報名
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var newRegistration = new Participation
                    {
                        CId = clientId,
                        ActivityId = activityId,
                        ParticipationDate = DateTime.Now,
                        ParticipationStatusId = 1
                    };
                    _context.Participations.Add(newRegistration);

                    // 如果活動需要扣除名額 (Capacity 不是 -1)
                    if (activity.Capacity > 0)
                    {
                        activity.Capacity--; // 名額減一
                        _context.Activities.Update(activity);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "恭喜您，報名成功！" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // 記錄錯誤 (例如使用 ILogger)
                    Console.Error.WriteLine($"報名失敗: {ex.Message}");
                    return Json(new { success = false, message = "報名失敗，請稍後再試。若問題持續請聯繫客服。" });
                }
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        #region 有修改的部分

        [HttpGet]
        public IActionResult Register()
        {
            UserRegistrationDto model = new UserRegistrationDto();

            if (TempData["PreFillName"] != null)
            {
                model.Name = TempData["PreFillName"] as string ?? string.Empty;
            }
            if (TempData["PreFillPhoneNumber"] != null)
            {
                model.PhoneNumber = TempData["PreFillPhoneNumber"] as string ?? string.Empty;
            }
            if (TempData["PreFillEmail"] != null)
            {
                model.Email = TempData["PreFillEmail"] as string ?? string.Empty;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Email == null || model.Password == null)
            {
                return View(model);
            }

            var registerResult = await _userService.RegisterNewUser(model.Name, model.PhoneNumber, model.Email, model.Password);

            if (registerResult.IsSuccess)
            {
                TempData["Result"] = "success";
                TempData["ShowModal"] = true;
                TempData["ResultMessage"] = registerResult.SuccessMessage;
                return RedirectToAction("Register");
            }

            else
            {
                TempData["Result"] = "fail";
                TempData["ShowModal"] = true;
                TempData["ResultMessage"] = registerResult.FailMessage;

                // 失敗時，將部分數據存入 TempData
                TempData["PreFillName"] = model.Name;
                TempData["PreFillPhoneNumber"] = model.PhoneNumber;
                TempData["PreFillEmail"] = model.Email;

                // 重定向到 GET 版本的 Register 避免重複提交表單問題
                return RedirectToAction("Register");
            }
        }

        #endregion

        #endregion
    }
}