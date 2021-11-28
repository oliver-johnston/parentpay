from flask import Flask
import read_parentpay
import pickle


app = Flask(__name__, static_url_path='')


@app.put('/bookings')
def refresh_bookings():
    bookings, date = read_parentpay.get_bookings()
    data = dict(bookings=bookings, dates=list(date))
    with open('bookings.pickle', 'wb') as outfile:
        pickle.dump(data, outfile)
    return data


@app.get('/bookings')
def get_bookings():
    with open('bookings.pickle', 'rb') as infile:
        return pickle.load(infile)


if __name__ == '__main__':
    app.run()
