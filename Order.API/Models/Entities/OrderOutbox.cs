using System.ComponentModel.DataAnnotations;

namespace Order.API.Models.Entities
{
    public class OrderOutbox
    {
        [Key]
        public Guid IdempotentToken { get; set; } // Geçici Olarak eklendi
        public DateTime OccuredOn { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}
