using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using test2.Areas.Frontend.Models.Dtos;
using test2.Controllers;
using test2.Models;
using test2.Models.ManagementModels.GmailSMTP;
using test2.Models.ManagementModels.Services;
using test2.Models.ManagementModels.ZhongXian.Appoimtment;
using test2.Models.ManagementModels.ZhongXian.AppoimtmentQuery;
using test2.Models.ManagementModels.ZhongXian.BookQuery;
using test2.Models.ManagementModels.ZhongXian.Borrow;
using test2.Models.ManagementModels.ZhongXian.BorrowQuery;
using test2.Models.ManagementModels.ZhongXian.Normal;
using test2.Models.ManagementModels.ZhongXian.RegisterBook;
using test2.Models.ManagementModels.ZhongXian.ReturnBook;

namespace test2.Areas.Backend.Controllers
{
    [Area("Backend")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public class ManageController : UserController
    {
        #region action
        private readonly Test2Context _context;
        EmailSender EmailSenders = new EmailSender();
        public ManageController(Test2Context context)
        {
            _context = context;
        }
        public IActionResult Index() { return View(); }
        #endregion

        //----葉忠憲處理部分------------------------------------------------------------------------------------------

        #region 預約管理&查詢
        // 預約管理_搜尋排列_partial
        public IActionResult AppointmentQuery()
        {
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_appointmentQueryPartial.cshtml");
        }
        //預約管理_查詢列表_partial
        [HttpGet]
        public async Task<IActionResult> AppointmentResult(AppoimtmentQueryFilter formData)
        {
            AppointmentQueryFinalSearch newclass = new AppointmentQueryFinalSearch();
            var final = await newclass.AppointmentQuerySearch(_context, formData);
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_appointmentResultPartial.cshtml", final);
        }
        // 取消預約 站內通知 + Email
        [HttpPatch]
        public async Task<IActionResult> CancelAppointment(int NotificationAppointmentId, int NotificationUser, string NotificationTextarea)
        {
            var Noticaionl = new Notification
            {
                CId = NotificationUser,
                Message = NotificationTextarea,
                NotificationDate = DateTime.UtcNow
            };
            var text = await _context.Reservations.Where(x => x.ReservationId == NotificationUser && x.CId == NotificationUser).ToListAsync();
            if (text.IsNullOrEmpty()) { Json(0); }

            var cancelAppointment = await _context.Reservations.FirstOrDefaultAsync(x => x.ReservationId == NotificationAppointmentId && x.CId == NotificationUser);

            if (cancelAppointment == null) { Json(0); }
            cancelAppointment!.ReservationStatusId = 4;
            await _context.Notifications.AddAsync(Noticaionl);
            await _context.SaveChangesAsync();

            //Email發送郵件 取得信箱、名字、書本
            var user = await _context.Reservations.Include(x => x.CIdNavigation).Include(x => x.Collection).Where(x => x.CIdNavigation.CId == NotificationUser).Select(result => new ReturnDTO
            {
                cName = result.CIdNavigation.CName,
                cAccount = result.CIdNavigation.CAccount,
                title = result.Collection.Title
            }).ToListAsync();
            //string subject = "【取消預約通知】";
            //string body = $"親愛的{user[0].cName}您好，您所預約的書籍《 {user[0].title} 》\r\n已於 {DateTime.Now.ToString("yyyy-MM-dd")} \r\n由本館管理員取消。\r\n取消原因： 考零分 。若您仍有借閱需求，歡迎重新進行預約。\r\n如有任何問題或需協助，敬請聯繫本館服務人員，我們將竭誠為您服務。感謝您的配合與理解！圖書館管理系統 敬上。";
            //await EmailSenders.SendAsync(user[0].cAccount!, subject, body);
            return Json(1);
        }

        #endregion

        #region 借閱查詢
        // 借閱查詢_搜尋排列_partial
        public IActionResult BorrowQuery()
        {
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_borrowQueryPartial.cshtml");
        }
        // 借閱查詢_查詢列表_partial
        [HttpGet]
        public async Task<IActionResult> BorrowResult(BorrowQueryFilter borrowForm)
        {
            var service = new BorrowQueryFinalSearch();
            var BorrowQueryViewModels2 = await service.BorrowQuerySeach(_context, borrowForm);
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_borrowResultPartial.cshtml", BorrowQueryViewModels2);
        }
        #endregion

        #region 借閱模式
        // 借書模式_partial
        public IActionResult BorrowMode()
        {
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_borrowModePartial.cshtml");
        }
        // 借書模式_借書
        [HttpPost]
        public async Task<IActionResult> BorrowSend(int borrwoMode_UserID, string borrwoMode_BookCode)
        {
            var UserId = await _context.Clients.Where(x => x.CId == borrwoMode_UserID).Select(y => new { y.CId, y.CName }).FirstOrDefaultAsync();
            if (UserId == null) { return Json(0); }
            ;
            var BookInfo = await _context.Books.Join(_context.Collections, bok => bok.CollectionId, col => col.CollectionId, (bok, col) => new { bok, col }).Where(x => x.bok.BookCode == borrwoMode_BookCode).Select(result => new { result.col.Title }).FirstOrDefaultAsync();

            var SqlResult = await _context.Set<MessageDTO>().FromSqlInterpolated($"EXEC BorrowResult {borrwoMode_UserID}, {borrwoMode_BookCode}").ToListAsync();
            var result = new ResultViewModel()
            {
                ResultCode = SqlResult[0].ResultCode,
                Message = SqlResult[0].Message,
                Cid = UserId.CId,
                cName = UserId.CName ?? "查無此借閱者!!",
                title = BookInfo?.Title ?? "查無此書本!!",
                bookcode = borrwoMode_BookCode
            };
            return PartialView("~/Areas/Backend/Views/Manage/BorrowModeContent.cshtml", result);
        }
        // 借書模式_借書人資訊
        [HttpGet]
        public async Task<IActionResult> BorrowUserMessage(int userId)
        {
            Debug.WriteLine("借書人資訊訊借書人資訊訊借書人資訊訊借書人資訊訊");
            var UserInfoamtion = await _context.Clients.Where(x => x.CId == userId).Select(result => new BorrowUser
            {
                cId = result.CId,
                cName = result.CName
            }).ToListAsync();
            if (UserInfoamtion.Count != 1) { return Json(false); }
            return PartialView("~/Areas/Backend/Views/Manage/BorrowModeUser.cshtml", UserInfoamtion);
        }
        // 借書模式_書本資訊
        [HttpGet]
        public async Task<IActionResult> BorrowBookMessage(string bookId)
        {
            var BookInformation = await _context.Set<BorrowBookInfomationDTO>().FromSqlInterpolated($"EXEC BookInfomationForBorrow {bookId}").ToListAsync();
            if (BookInformation.Count != 1) { return Json(false); }
            Debug.WriteLine(bookId);
            return PartialView("~/Areas/Backend/Views/Manage/BorrowModeBook.cshtml", BookInformation);
        }
        #endregion 借閱模式END

        #region 還書模式
        public IActionResult ReturnBookMode()
        {
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_returnBookPartial.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> ReturnBookSend(string ReturnBookCode)
        {
            ReturnBookClass returnBook = new ReturnBookClass();
            var resultViewModel = await returnBook.ReturnBookMethod(_context, ReturnBookCode);
            return PartialView("~/Areas/Backend/Views/Manage/ReturnBookContent.cshtml", resultViewModel);
        }
        #endregion 還書模式 END

        #region 預約模式

        public IActionResult AppointmentMode1()
        {
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_appoimtmentPartial.cshtml");
        }
        // 預約發送
        [HttpPost]
        public async Task<IActionResult> AppointmentMode1Send(int appointmentMode_UserID, int appointmentMode_BookId)
        {
            AppointmentModelSendClass AppointmentSends = new AppointmentModelSendClass(_context);
            var result = await AppointmentSends.AppointmentSend(appointmentMode_UserID, appointmentMode_BookId);
            string ViewUrl = "~/Areas/Backend/Views/Manage/AppoimtmentContent.cshtml";
            return PartialView(ViewUrl, result);
        }
        // 關鍵字搜尋
        [HttpGet]
        public async Task<IActionResult> AppointmentMode1Query(string keyWord, string state, int pageCount, int page = 1)
        {
            var result = await _context.Set<AppoimtmentKeywordDTO>().FromSqlInterpolated($"EXEC BookStatusDetail {keyWord}, {state}").ToListAsync();
            var totalcount = result.Count();
            if (totalcount == 0) { return Json(0); }
            var final = result.Skip((page - 1) * pageCount).Take(pageCount).ToList();
            var FinalRestul = new QueryViewModel()
            {
                AppoimtmentKeywordDTOs = final,
                PageCounts = new List<PageCount>() { new PageCount { TotalCount = result.Count(), CurrentPage = page, perPage = pageCount, } }
            };
            return PartialView("~/Areas/Backend/Views/Manage/AppoimtmentModeQuery.cshtml", FinalRestul);
        }
        #endregion

        #region 書籍登陸
        public async Task<IActionResult> BooksAdds()
        {
            var bookLanguages = await _context.Languages.ToListAsync();
            var bookTypes = await _context.Types.ToListAsync();
            LanguageAndTypeViewModel LanguageAndTypes = new LanguageAndTypeViewModel()
            {
                Language = bookLanguages,
                Type = bookTypes,
            };
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_registerPartial.cshtml", LanguageAndTypes);
        }
        [HttpPost]
        public async Task<IActionResult> BooksCreate(BookAddsClass formdata, IFormFile BookAdd_InputImg)
        {
            BookCreateClass NewBook = new BookCreateClass(_context);
            var result = await NewBook.ReturnBook(formdata, BookAdd_InputImg);
            return Json(result);
        }
        #endregion

        #region 書籍管理
        public async Task<IActionResult> BooksQuerys()
        {
            var bookLangue = await _context.Languages.ToListAsync();
            var bookType = await _context.Types.ToListAsync();
            LanguageAndTypeViewModel latvim = new LanguageAndTypeViewModel()
            {
                Language = bookLangue,
                Type = bookType
            };
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_bookQueryPartial.cshtml", latvim);
        }

        // 書籍管理查詢結果
        [HttpGet]
        public async Task<IActionResult> BooksQueryResult(BookQueryFormModel BookForm)
        {
            var BookQueryClass = new BookQueryResult(_context);
            var QueryResult = await BookQueryClass.BookQueryResultMethod(BookForm);

            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_bookQueryResultPartial.cshtml", QueryResult);
        }
        [HttpGet]
        // BookCode搜尋
        public async Task<IActionResult> BookQueryBookCode(int collecionId1)
        {
            List<BookQueryBookCodeDTO> bookCodeList = await _context.Books.Include(x => x.BookStatus).Where(x => x.CollectionId == collecionId1).Select(result => new BookQueryBookCodeDTO
            {
                CollectionId = collecionId1,
                BookCode = result.BookCode,
                BookStatus1 = result.BookStatus.BookStatus1,
                AccessionDate = result.AccessionDate
            }).OrderBy(x => x.BookCode).ToListAsync();

            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_bookCodePartial.cshtml", bookCodeList);
        }
        // 藏書編輯
        [HttpPatch]
        public async Task<MessageDTO> BookQueryEdit([FromBody] ColllectionEdit book)
        {
            BookQueryEditClass bookClass = new BookQueryEditClass(_context);
            var result = await bookClass.BookEdit(book);
            return result;
        }
        // 新增BookCode
        [HttpPost]
        public async Task<MessageDTO> BookQueryAddsBookCode([FromBody] Book newBookCode)
        {
            if (newBookCode == null) { return new MessageDTO() { ResultCode = 0, Message = "新增失敗" }; }
            var newbook = await _context.Books.Where(x => x.BookCode == newBookCode.BookCode).ToListAsync();
            if (newbook.FirstOrDefault() != null) { return new MessageDTO() { ResultCode = 0, Message = "BookCode重複了" }; }
            try
            {
                _context.Add(newBookCode);
                await _context.SaveChangesAsync();
                return new MessageDTO() { ResultCode = 1, Message = "新增成功" };
            }
            catch (Exception ex) { Debug.WriteLine("錯誤:" + ex); return new MessageDTO() { ResultCode = 0, Message = "發生錯誤，請稍後再試" }; }
        }
        // 更新書籍狀態、編碼
        [HttpPatch]
        public async Task<MessageDTO> BookQueryEditBookStatus([FromBody] Book books)
        {
            if (books == null) { return new MessageDTO() { ResultCode = 0, Message = "書本不存在" }; }
            try
            {
                var bookStar = await _context.Books.Where(x => x.BookCode == books.BookCode && x.CollectionId == books.CollectionId).ToListAsync();
                foreach (var bok in bookStar)
                {
                    bok.BookStatusId = books.BookStatusId;
                }
                await _context.SaveChangesAsync();
                return new MessageDTO() { ResultCode = 1, Message = "書本狀態更改成功" };
            }
            catch (Exception ex) { Debug.WriteLine("錯誤: " + ex); return new MessageDTO() { ResultCode = 0, Message = "發生錯誤，請稍後再試" }; }
        }

        // 刪除BookCode
        [HttpDelete]
        public async Task<MessageDTO> BookQueryDeleteBookCode(string bookCodeid)
        {
            var deleteBookCode = await _context.Books.Where(x => x.BookCode == bookCodeid).ToListAsync();
            var bookcode = await _context.Borrows.Include(x => x.Reservation).Where(x => x.BookId == deleteBookCode[0].BookId).ToListAsync();
            if (bookcode.Count() != 0) { return new MessageDTO() { ResultCode = 0, Message = $"{bookCodeid}已有紀錄無法刪除" }; }
            var collectionCount = await _context.Books.Where(x => x.CollectionId == deleteBookCode[0].CollectionId).ToListAsync();
            if (collectionCount.Count() == 1) { return new MessageDTO() { ResultCode = 0, Message = $"只剩一本無法刪除" }; }
            try
            {
                _context.Books.Remove(deleteBookCode![0]);
                await _context.SaveChangesAsync();
                return new MessageDTO() { ResultCode = 1, Message = $"成功刪除{bookCodeid}" };
            }
            catch (Exception ex)
            {
                return new MessageDTO() { ResultCode = 0, Message = $"刪除失敗: {ex}" };
            }
        }
        #endregion

        #region 通通用
        // 傳送通知
        [HttpPost]
        public async Task<IActionResult> Notification(int NotificationId, string NotificationType, string NotificationTextarea)
        {
            var user = await _context.Clients.FirstOrDefaultAsync(x => x.CId == NotificationId);
            if (user == null) { return Json("該借閱者不存在"); }
            var NotificationSend = new Notification()
            {
                CId = NotificationId,
                Message = $"【{NotificationType}】 {NotificationTextarea}",
                NotificationDate = DateTime.UtcNow
            };
            await _context.AddAsync(NotificationSend);
            await _context.SaveChangesAsync();

            // Email通知 
            //string subject = "【"+NotificationType+"】";
            //string body = NotificationTextarea;
            //await EmailSenders.SendAsync(user.CAccount, subject, body);
            return Json(1);
        }

        // 作者AutoComplete
        [HttpGet]
        public async Task<IActionResult> AuthorSearch(string authorLike)
        {
            var author = await _context.Authors.Where(x => x.Author1.Contains(authorLike)).ToListAsync();
            return Json(author);
        }
        // 類型
        [HttpGet]
        public async Task<IActionResult> TypeSearch(string type)
        {
            var Typets = await _context.Types.ToListAsync();
            var Typeid = await _context.Types.Where(x => x.Type1 == type).Select(result => new { id = result.TypeId }).ToListAsync();
            var TypeViewModel = new LanguageAndTypeViewModel()
            {
                id = Typeid[0].id,
                Type = Typets
            };
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_bookQueryType.cshtml", TypeViewModel);
        }
        // 語言
        [HttpGet]
        public async Task<IActionResult> LanguageSearch(string language)
        {
            var Languages = await _context.Languages.ToListAsync();
            var LanguageId = await _context.Languages.Where(x => x.Language1 == language).Select(result => new { id = result.LanguageId }).ToListAsync();
            var LanguageViewModel = new LanguageAndTypeViewModel()
            {
                id = LanguageId[0].id,
                Language = Languages
            };
            return PartialView("~/Areas/Backend/Views/Shared/_Partial/_bookQueryLanguage.cshtml", LanguageViewModel);
        }
        // 關鍵字動態搜尋
        [HttpGet]
        public async Task<IActionResult> KeyWordAuthorSearch(string keyword)
        {
            var bookTitle = await _context.Collections.Where(x => x.Title.Contains(keyword)).Select(re => new { Label = re.Title + "(書名)", Value = re.Title }).ToListAsync();
            var bookAuthor = await (from col in _context.Collections
                                    join auth in _context.Authors on col.AuthorId equals auth.AuthorId
                                    where auth.Author1.Contains(keyword)
                                    group auth by auth.Author1 into re
                                    select new
                                    {
                                        Label = re.Key + "(作者)",
                                        Value = re.Key
                                    }).ToListAsync();
            var autoComplete = bookTitle.Concat(bookAuthor).Take(10);
            return Json(autoComplete);
        }
        #endregion

        //------------------------------------------------------------------------------------------
    }
}