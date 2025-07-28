// Models/ActivityRegistration.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test2.Models
{
    [Table("ActivityRegistrations")] // 指定對應的資料庫表格名稱
    public partial class ActivityRegistration
    {
        [Key] // 指定為主鍵
        [Column("ActivityRegistrationId")] // 對應資料庫欄位名稱 (PascalCase)
        public int ActivityRegistrationId { get; set; }

        [Column("ClientId")] // 對應資料庫欄位名稱 (PascalCase)
        public int ClientId { get; set; }

        [Column("ActivityId")] // 對應資料庫欄位名稱 (PascalCase)
        public int ActivityId { get; set; }

        [Column("ActivityRegistrationDate", TypeName = "datetime")] // 對應資料庫欄位名稱和型別 (PascalCase)
        public DateTime ActivityRegistrationDate { get; set; } = DateTime.Now; // C# 端的預設值

        [Column("Status")] // 對應資料庫欄位名稱 (PascalCase)
        [StringLength(50)] // 與資料庫 NVARCHAR(50) 對應
        public string Status { get; set; } = "已報名"; // C# 端的預設值，與資料庫 DEFAULT '已報名' 對應

        // 導覽屬性 (Navigation Properties)
        public virtual Client Client { get; set; } = null!; // 每個報名記錄都必須有一個 Client
        public virtual Activity Activity { get; set; } = null!; // 每個報名記錄都必須有一個 Activity
    }
}