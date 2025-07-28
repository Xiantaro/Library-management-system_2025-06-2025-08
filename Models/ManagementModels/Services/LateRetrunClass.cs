using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using test2.Models.ManagementModels.GmailSMTP;
using test2.Models.ManagementModels.ZhongXian.Normal;

namespace test2.Models.ManagementModels.Services
{
    public class LateRetrunClass
    {
        private readonly Test2Context _context;
        public LateRetrunClass(Test2Context context)
        {
            _context = context;
        }
        EmailSender EmailSenders = new EmailSender();
        public async Task<MessageDTO> LateRetrunStart()
        {
            Debug.WriteLine("開始進行逾期還書檢查.....");
            // 檢查逾期、更改並站內通知
            var LateReturnList = await _context.Set<ReturnDTO>().FromSqlRaw($"EXEC LateReturn2Email").ToListAsync();
            if (LateReturnList[0].ResultCode == 0) { return new MessageDTO() { Message = "今天無逾期未還" }; }
            Debug.WriteLine("今天逾期人數: " + LateReturnList.Count);
            foreach (var x in LateReturnList)
            {
                Debug.WriteLine($"逾期還書者:{x.cName} _ 書名: {x.title} _ 信箱 : {x.cAccount}");
                //string subject = "【逾期警告通知】";
                //string body = $"親愛的 {x.cName}，您所借閱的  {x.title} \r\n已經逾期，請盡速還書。\r\n圖書館管理系統 敬上。";
                //await EmailSenders.SendAsync(x.cAccount!, subject, body);
            }

            return new MessageDTO() { Message = "逾期未還檢查結束!" };
        }
    }
}
