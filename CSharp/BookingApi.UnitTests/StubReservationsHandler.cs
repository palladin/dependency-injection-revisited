using Eff.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.BookingApi.UnitTests
{
    public class StubReservationsHandler : IReservationsInstrHandler
    {

        private readonly bool isInFuture;
        private readonly IReadOnlyCollection<Reservation> reservations;
        private readonly int id;

        public StubReservationsHandler(
            bool isInFuture,
            IReadOnlyCollection<Reservation> reservations,
            int id)
        {
            this.isInFuture = isInFuture;
            this.reservations = reservations;
            this.id = id;
        }

        public Task<ReservationsProgram<TResult>> Handle<TResult>(IsReservationInFuture instr, Func<bool, ReservationsProgram<TResult>> cont)
        {
            return Task.FromResult(cont(isInFuture));
        }

        public Task<ReservationsProgram<TResult>> Handle<TResult>(ReadReservations instr, Func<IReadOnlyCollection<Reservation>, ReservationsProgram<TResult>> cont)
        {
            return Task.FromResult(cont(reservations));
        }

        public Task<ReservationsProgram<TResult>> Handle<TResult>(CreateReservation instr, Func<int, ReservationsProgram<TResult>> cont)
        {
            return Task.FromResult(cont(id));
        }
    }
}
