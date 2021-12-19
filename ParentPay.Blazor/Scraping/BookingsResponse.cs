using System;
using System.Collections.Generic;

namespace ParentPay.Blazor.Scraping;

public class BookingsResponse
{
    public IList<Booking> Bookings { get; set; } = new List<Booking>();

    public IList<DateTime> Dates { get; set; } = new List<DateTime>();

    public IList<Child> Children { get; set; } = new List<Child>();

    public IList<Club> Clubs { get; set; } = new List<Club>();
}