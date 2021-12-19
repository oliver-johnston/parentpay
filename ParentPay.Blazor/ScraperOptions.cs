using System.Collections.Generic;
using ParentPay.Blazor.Scraping;

namespace ParentPay.Blazor;

public class ScraperOptions
{
    public IList<Club> Clubs { get; set; }
    public IList<Child> Children { get; set; }
    public IList<ExtraClub> ExtraClubs { get; set; }
}