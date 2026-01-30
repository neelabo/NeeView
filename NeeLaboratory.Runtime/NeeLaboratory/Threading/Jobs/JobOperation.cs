//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    public interface IJobOperation
    {
        JobState State { get; }
        ValueTask InvokeAsync(CancellationToken token);
    }


    /// <summary>
    /// Job Operation for Task-Based Job Engine
    /// </summary>
    /// <typeparam name="T">Return type of the job</typeparam>
    public class JobOperation<T> : BindableBase, IJobOperation
    {
        private readonly Func<CancellationToken, ValueTask<T>> _job;
        private JobState _state;


        public JobOperation(Func<CancellationToken, ValueTask<T>> job)
        {
            _job = job;
        }


        public JobState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public T? Result { get; private set; }


        public async ValueTask InvokeAsync(CancellationToken token)
        {
            try
            {
                State = JobState.Run;
                Result = await _job(token);
                State = JobState.Completed;
            }
            catch (OperationCanceledException)
            {
                State = JobState.Canceled;
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                State = JobState.Faulted;
                throw;
            }
        }

        public async ValueTask<T?> WaitAsync(CancellationToken token)
        {
            await this.WaitPropertyAsync(nameof(State), e => e.State.IsFinished(), token);
            return Result;
        }
    }


}
