using System;

namespace RGBSelcer.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = "Оформлен";

        public User? User { get; set; }
        public Car? Car { get; set; }

        public string StatusDisplay => Status;
        public string DateFormatted => PurchaseDate.ToString("dd.MM.yyyy HH:mm");
    }
}
