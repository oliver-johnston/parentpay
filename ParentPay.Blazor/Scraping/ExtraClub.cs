using System;
using System.Collections.Generic;

namespace ParentPay.Blazor.Scraping
{
    public class ExtraClub
    {
        public string Child { get; set; }
        public Club Club { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DayOfWeek? Day { get; set; }
        public IList<DateTime> Dates { get; set; }
    }
}