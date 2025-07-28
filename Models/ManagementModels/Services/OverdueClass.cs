using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Tls;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using test2.Models.ManagementModels.GmailSMTP;
using test2.Models.ManagementModels.ZhongXian.Normal;

namespace test2.Models.ManagementModels.Services
{
    public class OverdueClass
    {
        private readonly Test2Context _context;
        public OverdueClass(Test2Context context)
        {
            _context = context;
        }
        EmailSender EmailSenders = new EmailSender();
        public async Task<MessageDTO> OverdueStart()
        {
            Debug.WriteLine("開始進行取書逾期檢查.....");
            // 1. 
            var OverdueResult = await _context.Set<OverDueDTO>().FromSqlInterpolated($"EXEC OverDue").ToListAsync();
            if (OverdueResult[0].ResultCode == 0) { return new MessageDTO { Message = "今天沒有取書逾期" }; }
            Debug.WriteLine($"今天取書逾期人數: {OverdueResult.Count}");
            foreach (var x in OverdueResult)
            {
                Debug.WriteLine($"取書逾期者:{x.cName} _ 書名: {x.title} _ 信箱 : {x.cAccount}");
                //string subject = "【取書逾期通知】";
                //string body = $"親愛的 {x.cName} \r\n您所預約的 【{x.title}】 \r\n未於 {x.dueDateB!.Value.ToString("yyyy-MM-dd")} 前取書，\r\n系統已取消，如有需要請重新預約，謝謝。\r\n圖書館管理系統 敬上。";
                //await EmailSenders.SendAsync(x.cAccount!, subject, body);
            }
            // 2. 
            DataTable dt = new DataTable();
            dt.Columns.Add("collectionid", typeof(int));
            dt.Columns.Add("bookid", typeof(int));
            foreach (var user in OverdueResult)
            {
                dt.Rows.Add([user.collectionId, user.bookid]);
            }
            var param = new SqlParameter("@users", SqlDbType.Structured)
            {
                TypeName = "dbo.DueBookTVP",
                Value = dt
            };

            // 3.進行 EXEC => CheckReservationSchedule => 尋找到給下一位預約者、更改狀態、站內通知、回傳資訊 
            var CheckReservation = await _context.Set<ReturnDTO>().FromSqlRaw($"EXEC CheckReservationSchedule @users", param).ToListAsync();
            foreach (var x in CheckReservation)
            {
                Debug.WriteLine($"下一位預約者: {x.cName} _ 書名: {x.title} _ 信箱 : {x.cAccount}");
                //string subject2 = "【取書通知】";
                //string body2 = $"親愛的 {x.cName} \r\n，您所預約的書 【{x.title}】\r\n已可以借閱，請於{DateTime.Now.Date.AddDays(2).ToString("yyyy-MM-dd")}天內到本館借書。\r\n圖書館管理系統 敬上。";
                //await EmailSenders.SendAsync(x.cAccount!, subject2, body2);
            }  
            Debug.WriteLine("取書逾期檢查結束!");
            return new MessageDTO() {Message = "取書逾期檢查結束!" };
        }
    }
}
