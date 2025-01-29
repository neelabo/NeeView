using NeeLaboratory.Threading.Jobs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace NeeView.UnitTest
{

    public class SingleJobEngineTest
    {
        //private readonly ITestOutputHelper _output;

        //public SingleJobEngineTest(ITestOutputHelper testOutputHelper)
        //{
        //    _output = testOutputHelper;
        //}


        [Fact(Timeout = 1000)]
        public async Task JobCancelTest()
        {
            var sw = Stopwatch.StartNew();
            var job = new SampleJob();

            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                job.Cancel();
                await Task.Delay(500);
            });

            await job.WaitAsync();

            sw.Stop();
            Assert.Equal(NeeLaboratory.Threading.Jobs.JobState.Canceled, job.State);
            Assert.True(sw.ElapsedMilliseconds >= 500);
        }

        [Fact(Timeout = 1000)]
        public async Task JobDisposeTest()
        {
            var sw = Stopwatch.StartNew();
            var job = new SampleJob();

            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                job.Dispose();
                await Task.Delay(500);
            });

            await job.WaitAsync();
            sw.Stop();

            Assert.Equal(NeeLaboratory.Threading.Jobs.JobState.Canceled, job.State);
            Assert.True(sw.ElapsedMilliseconds >= 500);
        }

        [Fact]
        public async Task JobEngineTest()
        {
            var engine = new SingleJobEngine("Test");
            engine.StartEngine();

            var context = new SampleJobContext();

            var job1 = new SampleJob("job1", context, 1);
            engine.Enqueue(job1);
            var job2 = new SampleJob("job2", context, 2);
            engine.Enqueue(job2);

            await job1.WaitAsync();
            Assert.Equal(1, context.Total);

            await job2.WaitAsync();
            Assert.Equal(3, context.Total);

            engine.Dispose();
        }

        [Fact]
        public async Task JobEngineCancelTest()
        {
            var engine = new SingleJobEngine("Test");
            engine.StartEngine();

            var context = new SampleJobContext();

            var job1 = new SampleJob("job1", context, 1);
            engine.Enqueue(job1);
            var job2 = new SampleJob("job2", context, 2);
            engine.Enqueue(job2);

            await Task.Delay(100);
            job1.Cancel();

            await job1.WaitAsync();
            Assert.Equal(0, context.Total);

            await Task.Delay(100);
            job2.Cancel();

            await job2.WaitAsync();
            Assert.Equal(0, context.Total);

            engine.Dispose();
        }
    }


    public class SampleJobContext
    {
        public int Total { get; private set; }

        public void Add(int value)
        {
            Total += value;
        }
    }

    public class SampleJob : JobBase
    {

        public SampleJob(string name = "(anonymous)")
        {
            Name = name;
        }

        public SampleJob(string name, SampleJobContext context, int value) : this(name)
        {
            Name = name;
            Context = context;
            Value = value;
        }

        public string Name { get; }

        public SampleJobContext? Context { get; }
        public int Value { get; }


        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Delay(500, CancellationToken.None);

            token.ThrowIfCancellationRequested();
            Context?.Add(Value);
        }
    }


}
