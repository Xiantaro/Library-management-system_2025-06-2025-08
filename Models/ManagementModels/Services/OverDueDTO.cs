namespace test2.Models.ManagementModels.Services
{
    public class OverDueDTO
    {
        public int ResultCode { get; set; }
        public string? Message { get; set; }
        public int? reservationId { get; set; }
        public int? borrowId { get; set; }
        public int? cid { get; set; }
        public int? collectionId { get; set; }
        public int? bookid { get; set; }
        public string? cName { get; set; } 
        public string? title { get; set; }
        public string? cAccount { get; set; }
        public DateTime? dueDateB { get; set; } 
    }
}
