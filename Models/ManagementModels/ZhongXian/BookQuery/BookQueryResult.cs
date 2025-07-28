using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using test2.Models.ManagementModels.ZhongXian.Normal;

namespace test2.Models.ManagementModels.ZhongXian.BookQuery
{
    public class BookQueryResult
    {
        private readonly Test2Context _context;
        public BookQueryResult(Test2Context context)
        {
            _context = context;
        }
        public async Task<QueryViewModel> BookQueryResultMethod(BookQueryFormModel BookForm)
        {
            var QueryResult = _context.Set<BookQueryDTO>().FromSqlInterpolated($"SELECT * FROM BookQueryResultView()").AsQueryable();
            bool BookEmptyFIlter()
            {
                return BookForm.book_ISBN == null &&
                    BookForm.book_KeyWord == null &&
                    BookForm.book_Language == "ALLIN" &&
                    BookForm.book_Type == "ALLIN" &&
                    BookForm.book_initDate == null &&
                    BookForm.book_lastDate == null;
            }
            if (BookForm.book_ISBN != null) { QueryResult = QueryResult.Where(x => x.isbn!.StartsWith(BookForm.book_ISBN)     ); }
            if (BookForm.book_KeyWord != null) { QueryResult = QueryResult.Where(x => x.title.Contains(BookForm.book_KeyWord) || x.author.Contains(BookForm.book_KeyWord)); }
            if (BookForm.book_Language != "ALLIN"){ QueryResult = QueryResult.Where(x => x.language == BookForm.book_Language); }
            if (BookForm.book_Type != "ALLIN"){ QueryResult = QueryResult.Where(x => x.type == BookForm.book_Type); }
            #region Date 
            // init有 > last null
            if (BookForm.book_initDate != null && BookForm.book_lastDate == null) { QueryResult = QueryResult.Where(x => x.publishDate >= BookForm.book_initDate && x.publishDate <= DateTime.Now); }
            // init null < last 有 = 預設一年
            else if (BookForm.book_initDate == null && BookForm.book_lastDate != null) { QueryResult = QueryResult.Where(x => x.publishDate <= BookForm.book_lastDate && x.publishDate >= DateTime.Now.AddYears(-1)); }
            // init < last
            else if (BookForm.book_initDate <= BookForm.book_lastDate) { QueryResult = QueryResult.Where(x => x.publishDate <= BookForm.book_lastDate && x.publishDate >= BookForm.book_initDate); }
            // init > last
            else if (BookForm.book_initDate >= BookForm.book_lastDate) { QueryResult = QueryResult.Where(x => x.publishDate >= BookForm.book_lastDate && x.publishDate <= BookForm.book_initDate); }
            #endregion

            if (BookForm.borrow_orderBy == "desc") { QueryResult = QueryResult.OrderByDescending(x => x.publishDate); }
            else { QueryResult = QueryResult.OrderBy(x => x.publishDate); }
            // 初始載入 10年內的書籍
            if (BookEmptyFIlter()) { QueryResult = QueryResult.Where(x => DateTime.Now.AddMonths(-10) <= x.publishDate && DateTime.Now >= x.publishDate); }
            int dataCount = await QueryResult.CountAsync();
            var finalResult = await QueryResult.Skip((BookForm.page -1) * BookForm.borrow_perPage).Take(BookForm.borrow_perPage).ToListAsync();
            var finalViewModel = new QueryViewModel()
            {
                BookQueryDTOs = finalResult,
                PageCounts = new List<PageCount>() { new PageCount { TotalCount = dataCount , CurrentPage = BookForm.page, perPage = BookForm.borrow_perPage} }
            };
            return finalViewModel;
        }
    }
}
