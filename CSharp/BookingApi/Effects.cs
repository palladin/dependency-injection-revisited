using Eff.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.BookingApi
{
    public class IsReservationInFuture : Effect<bool>
    {
        public Reservation Reservation { get; set; }
        public IsReservationInFuture() : base("", "", 0) { }
    }

    public class ReadReservations : Effect<IReadOnlyCollection<Reservation>>
    {
        public DateTimeOffset Date { get; set; }
        public ReadReservations() : base("", "", 0)  { }
    }

    public class CreateReservation : Effect<int>
    {
        public Reservation Reservation { get; set; }
        public CreateReservation(): base("", "", 0) { }
    }

    public static class ReservationsRepository
    {
        public static IsReservationInFuture IsReservationInFuture(Reservation reservation)
            => new IsReservationInFuture() { Reservation = reservation };
        public static ReadReservations ReadReservations(DateTimeOffset date)
            => new ReadReservations { Date = date };
        public static CreateReservation Create(Reservation reservation)
            => new CreateReservation { Reservation = reservation };
    }
}
