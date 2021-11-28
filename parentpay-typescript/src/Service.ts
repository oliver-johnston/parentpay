export interface GetBookingsResponse {
  bookings: Booking[];
  dates: Date[];
}

export interface Booking {
  club: Club;
  child: Child;
  date: string;
  url: string;
}

export interface Club {
  club_id: string;
  club_type: string;
  name: string;
}

export interface Child {
  child_id: string;
  name: string;
}