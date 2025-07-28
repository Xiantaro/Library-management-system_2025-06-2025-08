using test2.Models.ManagementModels.ZhongXian.Normal;

namespace test2.Models.ManagementModels.ZhongXian.RegisterBook
{
    public class BookCreateClass
    {
        private Test2Context _context;
        public BookCreateClass(Test2Context context)
        {
            _context = context;
        }
        BookCodeListClass myBookCode = new BookCodeListClass();
        public async Task<MessageDTO> ReturnBook(BookAddsClass formdata, IFormFile BookAdd_InputImg)
        {

            bool IsbnUNIQUE = _context.Collections.Any(x => x.Isbn == formdata.BooksAdded_ISBM);
            if (IsbnUNIQUE == true) { return new MessageDTO() { ResultCode = 0, Message = "重複ISBM，請重新輸入" }; }
            if (BookAdd_InputImg == null && BookAdd_InputImg?.Length == 0) { return new MessageDTO() { ResultCode = 0, Message = "請放入封面!" }; }

            NewAuthor myNewAuthor = new NewAuthor(_context);
            var authorid = await myNewAuthor.CreateAuthor(formdata.BooksAdded_authorId, formdata.BooksAdded_authorName!);

            using var ms = new MemoryStream();
            BookAdd_InputImg!.CopyTo(ms);
            byte[] imageBytes = ms.ToArray();
            var newCollection = new Collection()
            {
                Title = formdata.BooksAdded_Title!,
                CollectionDesc = formdata.BooksAdded_Dec,
                TypeId = formdata.BooksAdded_Type,
                AuthorId = authorid,
                Translator = formdata.BooksAdded_translator,
                Publisher = formdata.BooksAdded_pushier!,
                LanguageId = formdata.BooksAdded_leng,
                Isbn = formdata.BooksAdded_ISBM!,
                PublishDate = formdata.BooksAdded_puDate,
                CollectionImg = imageBytes,
            };
            _context.Add(newCollection);
            await _context.SaveChangesAsync();
            var collectionId = newCollection.CollectionId;
            List<Book> bookList = myBookCode.BookCodeAddToList(formdata.BooksAdded_Type, collectionId, formdata.BooksAdded_inCount);
            _context.AddRange(bookList);
            await _context.SaveChangesAsync();
            return new MessageDTO() { ResultCode = 1, Message = "成功新增書籍!" };
        }
    }
}
