﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.BookingApi
{
    #region Instrs

    public abstract class ReservationsInstr : ICriticalNotifyCompletion
    {
        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }
        public void UnsafeOnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        public abstract Await<TReturn> Await<TReturn>(Func<ReservationsProgram<TReturn>> cont);
    }

    public abstract class ReservationsInstr<TResult> : ReservationsInstr
    {
        private bool hasResult;
        public bool IsCompleted => this.hasResult;

        private TResult result;
        public TResult GetResult() => result;

        public ReservationsInstr<TResult> GetAwaiter() => this;

        public void SetResult(TResult result)
        {
            this.hasResult = true;
            this.result = result;
        }
        public abstract Task<TResult> Accept(IReservationsInstrHandler handler);

        public override Await<TReturn> Await<TReturn>(Func<ReservationsProgram<TReturn>> cont)
        {
            return new Await<TResult, TReturn>(this, x => { this.SetResult(x); return cont(); });
        }
    }

    public class IsReservationInFuture : ReservationsInstr<bool>
    {
        public Reservation Reservation { get; set; }
        public IsReservationInFuture() : base() { }

        public override Task<bool> Accept(IReservationsInstrHandler handler)
        {
            return handler.Handle(this);
        }
    }

    public class ReadReservations : ReservationsInstr<IReadOnlyCollection<Reservation>>
    {
        public DateTimeOffset Date { get; set; }
        public ReadReservations() : base() { }

        public override Task<IReadOnlyCollection<Reservation>> Accept(IReservationsInstrHandler handler)
        {
            return handler.Handle(this);
        }
    }

    public class CreateReservation : ReservationsInstr<int>
    {
        public Reservation Reservation { get; set; }
        public CreateReservation() : base() { }

        public override Task<int> Accept(IReservationsInstrHandler handler)
        {
            return handler.Handle(this);
        }
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

    public interface IReservationsInstrHandler
    {
        Task<bool> Handle(IsReservationInFuture instr);
        Task<IReadOnlyCollection<Reservation>> Handle(ReadReservations instr);
        Task<int> Handle(CreateReservation instr);
    }

    #endregion

    #region Builder

    [AsyncMethodBuilder(typeof(ReservationsProgramBuilder<>))]
    public abstract class ReservationsProgram<TResult>
    {
    }

    public abstract class Await<TResult> : ReservationsProgram<TResult>
    {
        public abstract Task<ReservationsProgram<TResult>> Accept(IReservationsInstrHandler handler);
    }

    public class Await<TBind, TResult> : Await<TResult>
    {
        public ReservationsInstr<TBind> Instr { get; private set; }
        public Func<TBind, ReservationsProgram<TResult>> Cont { get; set; }
        
        public Await(ReservationsInstr<TBind> instr, Func<TBind, ReservationsProgram<TResult>> cont)
        {
            this.Instr = instr;
            this.Cont = cont;
        }

        public override async Task<ReservationsProgram<TResult>> Accept(IReservationsInstrHandler handler)
        {
            return Cont(await Instr.Accept(handler));
        }
    }

    public class Return<TResult> : ReservationsProgram<TResult>
    {
        public TResult Result { get; private set; }

        public Return(TResult result)
        {
            this.Result = result;
        }
    }

    public class Delay<TResult> : ReservationsProgram<TResult>
    {
        public Func<ReservationsProgram<TResult>> Func { get; private set; }

        public Delay(Func<ReservationsProgram<TResult>> func)
        {
            this.Func = func;
        }
    }

    #endregion

    #region Executor

    public static class EffExecutor
    {

        public static async Task<TResult> Run<TResult>(this ReservationsProgram<TResult> prg, IReservationsInstrHandler handler)
        {
            var result = default(TResult);
            var done = false;
            while (!done)
            {
                switch (prg)
                {
                    case Return<TResult> ret:
                        result = ret.Result;
                        done = true;
                        break;
                    case Delay<TResult> delay:
                        prg = delay.Func();
                        break;
                    case Await<TResult> _await:
                        prg = await _await.Accept(handler);
                        break;
                    default:
                        throw new NotSupportedException($"{prg.GetType().Name}");
                }
            }

            return result;
        }
    }

    #endregion
}
