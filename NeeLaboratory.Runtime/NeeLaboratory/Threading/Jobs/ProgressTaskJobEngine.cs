//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// Task-based Job Engine with Progress
    /// </summary>
    [LocalDebug]
    public partial class ProgressTaskJobEngine<TProgressContext> : TaskJobEngine
    {
        public IProgress<TProgressContext>? Progress { get; set; }

        public JobOperation<int> AddJob(Func<IProgress<TProgressContext>?, CancellationToken, ValueTask> job)
        {
            return AddJob(InnerJob);

            async ValueTask<int> InnerJob(CancellationToken token)
            {
                await job(Progress, token);
                return 0;
            }
        }

        public JobOperation<T> AddJob<T>(Func<IProgress<TProgressContext>?, CancellationToken, ValueTask<T>> job)
        {
            return AddJob(InnerJob);

            async ValueTask<T> InnerJob(CancellationToken token)
            {
                return await job(Progress, token);
            }
        }
    }
}
