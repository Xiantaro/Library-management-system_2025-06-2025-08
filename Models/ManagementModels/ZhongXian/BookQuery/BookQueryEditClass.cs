using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using test2.Models.ManagementModels.ZhongXian.Normal;
using test2.Models.ManagementModels.ZhongXian.RegisterBook;

namespace test2.Models.ManagementModels.ZhongXian.BookQuery
{
    public class BookQueryEditClass
    {
        private Test2Context _context;
        public BookQueryEditClass(Test2Context context)
        {
            _context = context;
        }
        public async Task<MessageDTO> BookEdit(ColllectionEdit book)
        {
            string img64 = book.CollectionImg!.Split(",")[1];
            byte[] newBookImg = Convert.FromBase64String(img64);
            if (book.Author == null || book.Author == "") { return new MessageDTO() { ResultCode = 0, Message = "作者不能為空!" }; }
            // 作者搜尋&新增
            var newAuthor = new NewAuthor(_context);
            var newAuthorid = 0;
            if (book.AuthorId == 0) { newAuthorid = await newAuthor.CreateAuthor(book.Author); }
            else { newAuthorid = await newAuthor.CreateAuthor(book.AuthorId, book.Author); }

            if (book == null) { return new MessageDTO() { ResultCode = 0, Message = "沒有該書籍!" }; }
            try
            {
                var CollectionX = await _context.Collections.Where(x => x.CollectionId == book.CollectionId).ToListAsync();
                foreach (var col in CollectionX)
                {
                    col.Title = book.Title;
                    col.CollectionDesc = book.CollectionDesc;
                    col.TypeId = book.TypeId;
                    col.Translator = book.Translator;
                    col.Publisher = book.Publisher;
                    col.LanguageId = book.LanguageId;
                    col.Isbn = book.Isbn;
                    col.PublishDate = book.PublishDate;
                    col.AuthorId = newAuthorid;
                    col.CollectionImg = newBookImg;
                }
                await _context.SaveChangesAsync();
                return new MessageDTO() { ResultCode = 1, Message = "成功修改書籍" };
            }
            catch (Exception ex) { Debug.WriteLine("錯誤: " + ex); return new MessageDTO() { ResultCode = 0, Message = "ISBM無法重複" }; }

        }
    }
}
