using System.Diagnostics;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using test2.Models.ManagementModels.GmailSMTP;
using test2.Models.ManagementModels.Services;
using test2.Models.ManagementModels.ZhongXian.Normal;

namespace test2.Models.ManagementModels.ZhongXian.ReturnBook
{
    public class ReturnBookClass
    {
        EmailSender EmailSenders = new EmailSender();
        public async Task<ResultViewModel> ReturnBookMethod(Test2Context context, string ReturnBookCode)
        {
            var retrunBookIsReal = await context.Borrows.Include(x => x.Book).ThenInclude(x => x.Collection).Include(x => x.CIdNavigation).Where(x => x.Book.BookCode == ReturnBookCode)
                .Select(re => new { re.CId, re.CIdNavigation.CName, re.Book.Collection.Title }).ToListAsync();
            if (retrunBookIsReal.IsNullOrEmpty()) { return new ResultViewModel { ResultCode = 0, Message ="並未找到借閱書籍，請重新輸入"}; }
            var resultMessage = await context.Set<MessageDTO2>().FromSqlInterpolated($"EXEC ReturnBook {ReturnBookCode}").ToListAsync();
            var resultViewModel = new ResultViewModel()
            {
                ResultCode = resultMessage[0].ResultCode,
                Message = resultMessage[0].Message,
                Cid = retrunBookIsReal[0].CId,
                cName = retrunBookIsReal[0].CName,
                title = retrunBookIsReal[0].Title,
                bookcode = ReturnBookCode,
            };

            // 取得下一位預約者資訊並發站內通知、更新狀態
            var nextReservation = await context.Set<ReturnBookDTO>().FromSqlInterpolated($"EXEC CheckBookIsReservation {resultMessage[0].collectionid}, {resultMessage[0].bookid}").ToListAsync();
            //Email發送 下一位預約者Emal =>  cName、cAccount、title
            if (!nextReservation[0].cName.IsNullOrEmpty())
            {
                //string subject = "【取書通知】";
                //string body = $"親愛的 {nextReservation[0].cName} \r\n您所預約的書籍 【{nextReservation[0].title}】 已可以借閱\r\n請於 {DateTime.Now.Date.AddDays(3).ToString("yyyy-MM-dd")}前到本館借書。\r\n圖書館管理系統 敬上。";
                //await EmailSenders.SendAsync(nextReservation[0].cAccount!, subject, body);
            }
            return resultViewModel;
        }

    }
}
