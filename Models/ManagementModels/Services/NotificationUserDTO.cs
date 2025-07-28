namespace test2.Models.ManagementModels.Services
{
    public class NotificationUserDTO
    {
        public int? Cid { get; set; } 
        public string? cName { get; set; }
        public string? Email { get; set; }
        public string? Title { get; set; }
        public  DateTime? DueDays { get; set; }
        public  int? Days { get; set; }
        public int? CollectionId { get; set; }
        public int? ResultCode { get; set; }
        public string? Message { get; set; }
        public int? bookid { get; set; }
    }
}
