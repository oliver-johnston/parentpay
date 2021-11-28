from time import sleep
import datetime
from webbot import Browser
from bs4 import BeautifulSoup
import re


class Club:
    def __init__(self, club_id, club_type, name):
        self.club_id = club_id
        self.club_type = club_type
        self.name = name


class Child:
    def __init__(self, child_id, name):
        self.child_id = child_id
        self.name = name


def login(web):
    web.go_to('https://app.parentpay.com/public/client/security/#/login')

    sleep(3)
    web.type('<username>>', into='username')
    web.type('<password>', into='password')
    web.click('Login')
    sleep(3)


def get_bookings():
    web = Browser()
    login(web)

    children = [
        Child(13513236, 'Hannah'),
        Child(16561619, 'Emily')
    ]

    clubs = [
        Club(71893, 'Breakfast', 'Full'),
        Club(71894, 'Breakfast', 'Half'),
        Club(71896, 'After School', 'Full'),
        Club(71897, 'After School', 'Half')
    ]

    dates = [datetime.date.today() + datetime.timedelta(days=7)]

    if datetime.date.today().weekday() < 4:
        dates.append(datetime.date.today())

    parsed_dates = set()

    bookings = []

    for date in dates:
        next_monday = date + datetime.timedelta(days=-date.weekday(), weeks=1)
        monday = next_monday + datetime.timedelta(days=-7)
        date_str = monday.strftime('%Y-%m-%d')
        for child in children:
            for club in clubs:
                url = f'https://app.parentpay.com/V3Payer4VBW3/Consumer/MB_MenuSelection2.aspx?bookingID={club.club_id}&ConsumerId={child.child_id}&date={date_str}'
                web.go_to(url)
                sleep(3)

                content = web.get_page_source()
                soup = BeautifulSoup(content, features='html.parser')

                paid_dates = [datetime.datetime.strptime(th['class'][0], 'date-%Y_%m_%d')
                              for th
                              in soup.find_all('th', class_=re.compile('date-'))
                              if 'chosen' in th['class']]

                bookings.extend([dict(club=vars(club), child=vars(child), date=date, url=url) for date in paid_dates])

                all_dates = [datetime.datetime.strptime(th['class'][0], 'date-%Y_%m_%d')
                             for th
                             in soup.find_all('th', class_=re.compile('date-'))]

                for d in all_dates:
                    parsed_dates.add(d)

    web.close_current_tab()

    return bookings, parsed_dates


if __name__ == '__main__':
    bookings, parsed_dates = get_bookings()

    for date in sorted(list(parsed_dates)):

        print(date.strftime('%A - %d/%m/%Y'))

        date_bookings = [booking for booking in bookings if booking['date'] == date]

        for booking in date_bookings:
            print(f"{booking['child']}: {booking['club']}")

        print()
