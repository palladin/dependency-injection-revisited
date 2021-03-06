﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.BookingApi
{
    public static class ReservationsProgram
    {
        // Functor
        public static IReservationsProgram<TResult> Select<T, TResult>(
            this IReservationsProgram<T> source,
            Func<T, TResult> selector)
        {
            return source.SelectMany(x => new Pure<TResult>(selector(x)));
        }

        // Monad
        public static IReservationsProgram<TResult> SelectMany<T, TResult>(
            this IReservationsProgram<T> source,
            Func<T, IReservationsProgram<TResult>> selector)
        {
            return source.Accept(
                new SelectManyReservationsProgramVisitor<T, TResult>(
                    selector));
        }

        private class SelectManyReservationsProgramVisitor<T, TResult> :
            IReservationsProgramVisitor<T, IReservationsProgram<TResult>>
        {
            private readonly Func<T, IReservationsProgram<TResult>> selector;

            public SelectManyReservationsProgramVisitor(
                Func<T, IReservationsProgram<TResult>> selector)
            {
                this.selector = selector;
            }

            public IReservationsProgram<TResult> VisitFree(
                IReservationsInstruction<IReservationsProgram<T>> i)
            {
                return new Free<TResult>(i.Select(p => p.SelectMany(selector)));
            }

            public IReservationsProgram<TResult> VisitPure(T x)
            {
                return selector(x);
            }
        }

        public static IReservationsProgram<TResult> SelectMany<T, U, TResult>(
            this IReservationsProgram<T> source,
            Func<T, IReservationsProgram<U>> k,
            Func<T, U, TResult> s)
        {
            return source
                .SelectMany(x => k(x)
                    .SelectMany(y => new Pure<TResult>(s(x, y))));
        }

        // Lifts
        public static IReservationsProgram<bool> IsReservationInFuture(Reservation reservation)
        {
            return new Free<bool>(
                new IsReservationInFuture<IReservationsProgram<bool>>(
                    reservation,
                    x => new Pure<bool>(x)));
        }

        public static IReservationsProgram<IReadOnlyCollection<Reservation>> ReadReservations(
            DateTimeOffset date)
        {
            return new Free<IReadOnlyCollection<Reservation>>(
                new ReadReservations<IReservationsProgram<IReadOnlyCollection<Reservation>>>(
                    date,
                    x => new Pure<IReadOnlyCollection<Reservation>>(x)));
        }

        public static IReservationsProgram<int> Create(Reservation reservation)
        {
            return new Free<int>(
                new Create<IReservationsProgram<int>>(
                    reservation,
                    x => new Pure<int>(x)));
        }
    }
}
