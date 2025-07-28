namespace test2.Models.ManagementModels.ZhongXian.BorrowQuery
{
    public class BorrowQueryFilter
    {
        public int? borrow_BorrowID {get;set;}
        public int? borrow_UserID { get; set; }
        public string? borrow_bookCode { get; set; }
        public string? borrow_state { get; set; } 
        public DateTime? borrow_initDate { get; set; }
        public DateTime? borrow_lastDate { get; set; }
        public int borrow_perPage { get; set; } 
        public string? borrow_OrderDate { get; set; } 
        public string? borrow_orderBy { get; set; } 
        public int page { get; set; }
    }
}
