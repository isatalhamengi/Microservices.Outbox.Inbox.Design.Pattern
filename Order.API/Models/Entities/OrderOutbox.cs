using System.ComponentModel.DataAnnotations;

namespace Order.API.Models.Entities
{
    public class OrderOutbox
    {
        public int Id { get; set; } // Geçici Olarak eklendi
        public DateTime OccuredOn { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}
