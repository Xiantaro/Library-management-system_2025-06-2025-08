namespace test2.Models.ManagementModels.ZhongXian.BookQuery
{
    public class ColllectionEdit
    {
        public int CollectionId { get; set; }
        public string Title { get; set; } = null!;
        public string? CollectionDesc { get; set; }
        public string? CollectionImg { get; set; }
        public int TypeId { get; set; }
        public string? Translator { get; set; }
        public string Publisher { get; set; } = null!;
        public int LanguageId { get; set; }
        public string Isbn { get; set; } = null!;
        public DateTime PublishDate { get; set; }
        public int AuthorId { get; set; }
        public string? Author {get;set;}
    }
}
