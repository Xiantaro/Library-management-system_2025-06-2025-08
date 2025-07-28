using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using test2.Models.ManagementModels.GmailSMTP;
using test2.Models.ManagementModels.ZhongXian.Normal;

namespace test2.Models.ManagementModels.ZhongXian.Appoimtment
{
    public class AppointmentModelSendClass
    {
        private readonly Test2Context _context;
        EmailSender EmailSenders = new EmailSender();
        public AppointmentModelSendClass(Test2Context context)
        {
            _context = context;
        }
        public async Task<ResultViewModel> AppointmentSend(int appointmentMode_UserID, int appointmentMode_BookId)
        {
            var User = await _context.Clients.Where(x => x.CId == appointmentMode_UserID).ToListAsync();
            if (User?.Count == 0) { return new ResultViewModel() { ResultCode = 0, Message= "該名借閱者不存在，請重新輸入" }; }
            var bookId = await _context.Collections.Where(x => x.CollectionId == appointmentMode_BookId).Select(re => new { re.Title }).ToListAsync();
            if (bookId?.Count == 0) { return new ResultViewModel() { ResultCode = 0, Message = "該本書籍不存在，請重新輸入" }; }
            var ResultMessage = await _context.Set<MessageDTO>().FromSqlInterpolated($"EXEC AppointmentMode {appointmentMode_UserID}, {appointmentMode_BookId}").ToListAsync();
            var result = new ResultViewModel()
            {
                ResultCode = ResultMessage[0].ResultCode,
                Message = ResultMessage[0].Message,
                Cid = appointmentMode_UserID,
                cName = User?[0].CName,
                title = bookId?[0].Title,
            };
            //Email發送
            if (result.ResultCode == 1)
            {
                //string subject = "【預約成功通知】";
                //string body = $"親愛的 {User![0].CName}，您已於 {DateTime.Now.Date.ToString("yyyy-MM-dd")} \r\n預約了《{bookId![0].Title}》請耐心等候通知。\r\n圖書館管理系統 敬上。";
                //await EmailSenders.SendAsync(User[0].CAccount, subject, body);
            }
            return result;
        }
    }
}
