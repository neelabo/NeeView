//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeLaboratory.Threading.Jobs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 非同期処理をJOBとして順番に実行する
    /// </summary>
    /// <remake>
    /// 時間のかかる初期化処理を非同期でおこなうときに使用する
    /// </remake>
    [LocalDebug]
    public partial class ProcessJobEngine : ProgressTaskJobEngine<ProgressContext>
    {
        private static readonly Lazy<ProcessJobEngine> _current = new();
        public static ProcessJobEngine Current => _current.Value;

        public JobOperation<int> AddJob(string name, Action job)
        {
            return AddJob(InnerJob);

            async Task InnerJob(IProgress<ProgressContext>? progress, CancellationToken token)
            {
                progress?.Report(new ProgressContext(name));
                job.Invoke();
            }
        }

        public JobOperation<int> AddJob(string name, Func<CancellationToken, Task> job)
        {
            return AddJob(InnerJob);

            async Task InnerJob(IProgress<ProgressContext>? progress, CancellationToken token)
            {
                progress?.Report(new ProgressContext(name));
                await job.Invoke(token);
            }
        }
    }
}
