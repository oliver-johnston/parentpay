import React from "react";
import "./App.css";
import SyncLoader from "react-spinners/SyncLoader";
import * as _ from "lodash";
import { Booking, GetBookingsResponse } from "./Service";
import { DateTime } from "luxon";

class App extends React.Component<{}> {
  state = {
    bookings: [],
    dates: [],
    loading: false,
  };

  componentDidMount() {
    this.updateBookings("GET");
  }

  refreshBookings = () => {
    this.updateBookings("PUT");
  };

  updateBookings = (method: string = "GET") => {
    return fetch("/bookings", { method: method })
      .then((res) => res.json())
      .then((data: GetBookingsResponse) => {
        this.setState({
          bookings: data.bookings,
          dates: data.dates,
          loading: false,
        });
      })
      .catch((x: any) => {
        console.error(x);
        this.setState({ loading: false });
      });
  };

  render() {
    const children = _.chain(this.state.bookings)
      .map((x: Booking) => x.child.name)
      .sortedUniq()
      .value();

    return (
      <div className="container-fluid">
        <button type="button" className="btn btn-primary" onClick={this.refreshBookings}>
          Reload from Parent Pay
        </button>
        <SyncLoader loading={this.state.loading} />
        <div>
          <h2>Booking List</h2>
          {_.chain(this.state.bookings)
            .groupBy((x: Booking) => x.club.club_type)
            .map((clubBookings: Booking[], clubType: string) => {
              return (
                <div>
                  <h3>{clubType}</h3>
                  <table className="table table-striped">
                    <tr>
                      <th>Date</th>
                      {children.map((c) => {
                        return <th>{c}</th>;
                      })}
                    </tr>
                    {_.chain(this.state.dates)
                      .sortBy((date) => DateTime.fromHTTP(date))
                      .map((date) => {
                        const dateBookings = clubBookings.filter((b) => b.date === date);
                        return (
                          <tr>
                            <td>{DateTime.fromHTTP(date).toLocaleString({ month: "long", day: "numeric" })}</td>
                            {children.map((child) => {
                              const childBookings = dateBookings.filter((b) => b.child.name === child);
                              if (childBookings.length === 0) {
                                return <td className="table-warning">None</td>;
                              }

                              return (
                                <td className={childBookings.length > 1 ? "table-danger" : ""}>
                                  {childBookings.map((b) => {
                                    return <p>{b.club.name}</p>;
                                  })}
                                </td>
                              );
                            })}
                          </tr>
                        );
                      })}
                  </table>
                </div>
              );
            })}
        </div>
        <div>
          <h2>Links</h2>
          {_.chain(this.state.bookings)
            .map((x: Booking) => ({ url: x.url, description: `${x.child.name} - ${x.club.club_type} - ${x.club.name}` }))
            .sortedUniqBy((x) => x.description)
            .map((x) => {
              return (
                <li>
                  <a href={x.url} target="_blank">
                    {x.description}
                  </a>
                </li>
              );
            })}
        </div>
      </div>
    );
  }
}

export default App;
