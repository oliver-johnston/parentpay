using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace ParentPay.Blazor.Scraping
{
    public class ParentPayScraper : IParentPayScraper
    {
        // TODO - cache this some other way
        private static BookingsResponse _scrapedBookings = new BookingsResponse();

        private readonly ScraperOptions _options;

        public ParentPayScraper(IOptions<ScraperOptions> options)
        {
            _options = options.Value;
        }

        public Task<BookingsResponse> GetBookingsAsync()
        {
            return Task.FromResult(_scrapedBookings);
        }

        public async Task<BookingsResponse> ReloadBookingsAsync(DateTime weekDate, string username, string password)
        {
            await using var browser = await CreateBrowserAsync();
            await using var page = await browser.NewPageAsync();

            await LoginAsync(page, username, password);

            var dates = new List<DateTime>();
            var bookings = new List<Booking>();

            foreach (var child in _options.Children)
            {
                foreach (var club in _options.Clubs)
                {
                    var url =
                        $"https://app.parentpay.com/V3Payer4VBW3/Consumer/MB_MenuSelection2.aspx?bookingID={club.Id}&ConsumerId={child.Id}&date={weekDate:yyyy-MM-dd}";
                    await page.GoToAsync(url);
                    await page.WaitForSelectorAsync("th[class|=date]");

                    var headers = await page.QuerySelectorAllAsync("th[class|=date]");

                    foreach (var header in headers)
                    {
                        var classes = (await (await header.GetPropertyAsync("className"))
                            .JsonValueAsync<string>()).Split(" ");

                        var dateStr = classes.First(x => x.StartsWith("date"))
                            .Replace("date-", "");
                        var date = DateTime.ParseExact(dateStr, "yyyy_MM_dd", null, DateTimeStyles.None);

                        dates.Add(date);

                        if (classes.Contains("chosen"))
                        {
                            bookings.Add(new Booking
                            {
                                Child = child,
                                Club = club,
                                Date = date,
                                Url = url
                            });
                        }
                    }
                }
            }

            foreach (var date in dates.Distinct())
            {
                foreach (var club in _options.ExtraClubs)
                {
                    if ((club.StartDate != null
                         && club.EndDate != null
                         && club.StartDate <= date
                         && date <= club.EndDate
                         && club.Day == date.DayOfWeek)
                        || (club.Dates != null && club.Dates.Contains(date)))
                    {
                        bookings.Add(new Booking
                        {
                            Child = _options.Children.First(c => c.Name == club.Child),
                            Club = club.Club,
                            Date = date
                        });
                    }
                }
            }

            _scrapedBookings = new BookingsResponse
            {
                Bookings = bookings,
                Dates = dates.Distinct().OrderBy(x => x).ToList(),
                Children = _options.Children,
                Clubs = _options.Clubs
            };
            return _scrapedBookings;
        }

        private async Task<Browser> CreateBrowserAsync()
        {
            var pathsToTry = new[]
            {
                "/usr/bin/chromium-browser",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
            };
            var path = pathsToTry.First(File.Exists);
            return await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = path,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });
        }

        private async Task LoginAsync(Page page, string username, string password)
        {
            await page.GoToAsync("https://app.parentpay.com/public/client/security/#/login");
            await page.WaitForSelectorAsync("#login");
            await page.TypeAsync("#email", username);
            await page.TypeAsync("#password", password);
            await page.ClickAsync("#login");
            await page.WaitForNavigationAsync();
        }
    }

    public interface IParentPayScraper
    {
        Task<BookingsResponse> GetBookingsAsync();
        Task<BookingsResponse> ReloadBookingsAsync(DateTime weekDate, string username, string password);
    }
}