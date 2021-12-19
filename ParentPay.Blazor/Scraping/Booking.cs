using System;

namespace ParentPay.Blazor.Scraping
{
    public class Booking
    {
        public Child Child { get; set; }
        public Club Club { get; set; }
        public DateTime Date { get; set; }
        public string Url { get; set; }
    }
}