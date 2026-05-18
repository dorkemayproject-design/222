using System;
using System.Collections.Generic;

namespace RGBSelcer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<Purchase> Purchases { get; set; } = new();
    }
}
