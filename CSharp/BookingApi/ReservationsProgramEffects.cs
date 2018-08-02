using System;
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

        public abstract Task Accept(IReservationsInstrHandler handler);
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
    }

    public class IsReservationInFuture : ReservationsInstr<bool>
    {
        public Reservation Reservation { get; set; }
        public IsReservationInFuture() : base() { }

        public override Task Accept(IReservationsInstrHandler handler)
        {
            return handler.Handle(this);
        }
    }

    public class ReadReservations : ReservationsInstr<IReadOnlyCollection<Reservation>>
    {
        public DateTimeOffset Date { get; set; }
        public ReadReservations() : base() { }

        public override Task Accept(IReservationsInstrHandler handler)
        {
            return handler.Handle(this);
        }
    }

    public class CreateReservation : ReservationsInstr<int>
    {
        public Reservation Reservation { get; set; }
        public CreateReservation() : base() { }

        public override Task Accept(IReservationsInstrHandler handler)
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
        Task Handle(IsReservationInFuture instr);
        Task Handle(ReadReservations instr);
        Task Handle(CreateReservation instr);
    }

    #endregion

    #region Builder

    [AsyncMethodBuilder(typeof(ReservationsProgramBuilder<>))]
    public abstract class ReservationsProgram<TResult>
    {
    }

    public class Await<TResult> : ReservationsProgram<TResult>
    {
        public ReservationsInstr Instr { get; private set; }
        public Func<ReservationsProgram<TResult>> Cont { get; set; }
        
        public Await(ReservationsInstr instr, Func<ReservationsProgram<TResult>> cont)
        {
            this.Instr = instr;
            this.Cont = cont;
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
                        await _await.Instr.Accept(handler);
                        prg = _await.Cont();
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
