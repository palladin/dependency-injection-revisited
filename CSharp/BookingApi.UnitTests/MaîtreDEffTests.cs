using AutoFixture.Xunit2;
using Eff.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.BookingApi.UnitTests
{
    public class MaîtreDEffTests
    {
        [Theory, BookingApiTestConventions]
        public void TryAcceptReturnsReservationIdInHappyPathScenario(
            Reservation reservation,
            IReadOnlyCollection<Reservation> reservations,
            MaîtreDEffects sut,
            int excessCapacity,
            int expected)
        {
            var reservedSeats = reservations.Sum(r => r.Quantity);
            reservation.IsAccepted = false;
            sut = sut.WithCapacity(
                reservedSeats + reservation.Quantity + excessCapacity);

            var actual = sut.TryAccept(reservation);

            Assert.Equal(
                expected,
                actual.Run(new StubReservationsHandler(true, reservations, expected)).Result);
            Assert.True(reservation.IsAccepted);
        }

        [Theory, BookingApiTestConventions]
        public void TryAcceptReturnsNullOnReservationInThePast(
            Reservation reservation,
            IReadOnlyCollection<Reservation> reservations,
            int id,
            MaîtreDEffects sut)
        {
            reservation.IsAccepted = false;

            var actual = sut.TryAccept(reservation);

            Assert.Null(
                actual.Run(new StubReservationsHandler(false, reservations, id)).Result);
            Assert.False(reservation.IsAccepted);
        }

        [Theory, BookingApiTestConventions]
        public void TryAcceptReturnsNullOnInsufficientCapacity(
            Reservation reservation,
            IReadOnlyCollection<Reservation> reservations,
            int id,
            MaîtreDEffects sut)
        {
            reservation.IsAccepted = false;
            var reservedSeats = reservations.Sum(r => r.Quantity);
            sut = sut.WithCapacity(reservedSeats + reservation.Quantity - 1);

            var actual = sut.TryAccept(reservation);

            Assert.Null(
                actual.Run(new StubReservationsHandler(true, reservations, id)).Result);
            Assert.False(reservation.IsAccepted);
        }
    }
}
