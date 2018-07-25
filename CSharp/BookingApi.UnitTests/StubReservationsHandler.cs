using Eff.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.BookingApi.UnitTests
{
    public class StubReservationsHandler : EffectHandler
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

        public override async Task Handle<TResult>(IEffect<TResult> effect)
        {
            switch (effect)
            {
                case IsReservationInFuture isReservationInFuture:
                    isReservationInFuture.SetResult(isInFuture);
                    break;
                case ReadReservations readReservations:
                    readReservations.SetResult(reservations);
                    break;
                case CreateReservation createReservation:
                    createReservation.SetResult(id);
                    break;
                default:
                    throw new EffException($"Unhandled effect {effect.GetType().Name}");
            }
        }
    }
}
