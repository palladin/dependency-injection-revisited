﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.BookingApi
{
    public static class ReservationsInstruction
    {
        public static IReservationsInstruction<TResult> Select<T, TResult>(
            this IReservationsInstruction<T> source,
            Func<T, TResult> selector)
        {
            return source.Match(
                new SelectReservationsInstructionParameters<T, TResult>(
                    selector));
        }

        private class SelectReservationsInstructionParameters<T, TResult> :
            IReservationsInstructionParameters<T, IReservationsInstruction<TResult>>
        {
            private readonly Func<T, TResult> selector;

            public SelectReservationsInstructionParameters(
                Func<T, TResult> selector)
            {
                this.selector = selector;
            }

            public IReservationsInstruction<TResult> IsReservationInFuture(Tuple<Reservation, Func<bool, T>> t)
            {
                return new IsReservationInFuture<TResult>(new Tuple<Reservation, Func<bool, TResult>>(
                    t.Item1,
                    b => selector(t.Item2(b))));
            }

            public IReservationsInstruction<TResult> ReadReservations(Tuple<DateTimeOffset, Func<IReadOnlyCollection<Reservation>, T>> t)
            {
                return new ReadReservations<TResult>(new Tuple<DateTimeOffset, Func<IReadOnlyCollection<Reservation>, TResult>>(
                    t.Item1,
                    d => selector(t.Item2(d))));
            }

            public IReservationsInstruction<TResult> Create(Tuple<Reservation, Func<int, T>> t)
            {
                return new Create<TResult>(new Tuple<Reservation, Func<int, TResult>>(
                    t.Item1,
                    r => selector(t.Item2(r))));
            }
        }
    }
}
