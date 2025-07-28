using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.Data;
using System.Diagnostics;
using System.Xml.Linq;
using test2.Models.ManagementModels.GmailSMTP;
using test2.Models.ManagementModels.ZhongXian.Normal;

namespace test2.Models.ManagementModels.Services
{
    public class LateReturnOneToThreeClass
    {
        private readonly Test2Context _context;
        public LateReturnOneToThreeClass(Test2Context context)
        {
            _context = context;
        }
        EmailSender EmailSenders = new EmailSender();
        public async Task<MessageDTO> LateReturnStartOnToThree()
        {
            Debug.WriteLine("開始進行即將逾期檢查.....");
            // 逾期提醒前一天、前三天
            List<NotificationUserDTO> users = await _context.Borrows
                .Include(x => x.CIdNavigation).Include(x => x.Book).ThenInclude(x => x.Collection)
                .Where(x => x.BorrowStatusId == 2 && 
                x.DueDateB.Date == DateTime.Today.AddDays(1) ||
                x.DueDateB.Date == DateTime.Today.AddDays(3)
                )
                .Select(result => new NotificationUserDTO()
                {
                    Cid = result.CIdNavigation.CId,
                    cName = result.CIdNavigation.CName,
                    Title = result.Book.Collection.Title,
                    Days = (int)(result.DueDateB.Date -  DateTime.Today).TotalDays,
                    DueDays = result.DueDateB,
                    Email = result.CIdNavigation.CAccount
                }).ToListAsync();
            Debug.WriteLine($"今天即將逾期通知人數:{users.Count}");
            DataTable dt = new DataTable();
            dt.Columns.Add("Cid", typeof(int));
            dt.Columns.Add("ClientName", typeof(string));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("Days", typeof(int));
            dt.Columns.Add("DueDays", typeof(DateTime));
            dt.Columns.Add("Email", typeof(string));
            foreach (var user in users)
            {
                var TitleSubstring = user.Title!.Length > 50 ? user.Title.Substring(0, 50) : user.Title;
                dt.Rows.Add([user.Cid, user.cName, TitleSubstring, user.Days, user.DueDays, user.Email]);
            }
            var param = new SqlParameter("@users", SqlDbType.Structured)
            {
                TypeName = "dbo.OverDueOneToThreeTVP",
                Value = dt
            };

            // 站內通知 + Email
            await _context.Set<MessageDTO>().FromSqlRaw($"EXEC NotificationAboutToExpireToEmail @users", param).ToListAsync();
            foreach (var user in users)
            {
                Debug.WriteLine($"借閱者: {user.cName} _ 書籍 {user.Title} _ 歸還日期: {user.DueDays!.Value.Date.ToString("yyyy-MM-dd")} _ 信箱: {user.Email}");
                //string subject = "【即將逾期通知】";
                //string body = $"親愛的 {user.cName}，您所借閱的  {user.Title} \r\n 將於 {user.DueDays!.Value.Date.ToString("yyyy-MM-dd")} 逾期，距離還書期限僅剩 {user.Days} 天。\r\n圖書館管理系統 敬上。";
                //await EmailSenders.SendAsync(user.Email!, subject, body);

            }
            return new MessageDTO() { Message = "即將逾期檢查結束!" };
        }
    }
}
