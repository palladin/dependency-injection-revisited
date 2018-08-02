using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.BookingApi
{

    public class ReservationsProgramBuilder<TResult>
    {
        private ReservationsProgram<TResult> prg;
        
        private Func<ReservationsProgram<TResult>> cont;

        public static ReservationsProgramBuilder<TResult> Create()
        {
            return new ReservationsProgramBuilder<TResult>();
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            var state = stateMachine;
            this.cont = () =>
            {
                state.MoveNext();
                return this.prg;
            };
            this.prg = new Delay<TResult>(cont);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {

        }

        public void SetResult(TResult result)
        {
            this.prg = new Return<TResult>(result);
        }

        public void SetException(Exception exception)
        {
            throw exception;
        }

        public ReservationsProgram<TResult> Task => this.prg;

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ReservationsInstr
            where TStateMachine : IAsyncStateMachine
        {
            this.prg = new Await<TResult>(awaiter, this.cont);
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ReservationsInstr
            where TStateMachine : IAsyncStateMachine
        {
            this.prg = new Await<TResult>(awaiter, this.cont);
        }

    }
}
