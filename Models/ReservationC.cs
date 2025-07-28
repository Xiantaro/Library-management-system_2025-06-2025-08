using System.ComponentModel.DataAnnotations;

namespace test2.Models
{
    public class ReservationC
    {
        [Key]
        public string cId { get; set; } = string.Empty;
        public string collectionId { get; set; } = string.Empty;
        public DateTime reservationDate { get; set; }
        public string reservationStatusId { get; set; } = string.Empty;
    }
}
