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

        public async Task Handle(IsReservationInFuture instr)
        {
            instr.SetResult(isInFuture);
        }

        public async Task Handle(ReadReservations instr)
        {
            instr.SetResult(reservations);
        }

        public async Task Handle(CreateReservation instr)
        {
            instr.SetResult(id);
        }
    }
}
