using NeeLaboratory;
using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Xunit.Abstractions;

namespace NeeView.UnitTest
{
    public class Sandbox
    {
        private readonly ITestOutputHelper _output;

        public Sandbox(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
        }

        [Fact(Timeout = 2000)]
        public async Task EventFlagTest()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var job = Job.Create(null!, cancellationTokenSource.Token);

            _output.WriteLine("start...");
            var sw = Stopwatch.StartNew();
            _ = Task.Run(async () =>
            {
                _output.WriteLine("task start");
                await Task.Delay(1000);
                _output.WriteLine("job disposed.");
                job.Dispose();
            });

            _output.WriteLine("wait...");
            await job.WaitAsync(-1, CancellationToken.None);
            sw.Stop();
            _output.WriteLine($"done. {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void DispatcherTest()
        {
            Dispatcher? myDispatcher = null;

            var dispatcherReadyEvent = new ManualResetEvent(false);

            new Thread(new ThreadStart(() =>
            {
                myDispatcher = Dispatcher.CurrentDispatcher;
                dispatcherReadyEvent.Set();
                Dispatcher.Run();
            })).Start();

            dispatcherReadyEvent.WaitOne();
            if (myDispatcher is null) throw new InvalidOperationException();

            myDispatcher.Invoke(() => _output.WriteLine("invoke 1"));
            myDispatcher.InvokeShutdown();

            Assert.Throws<TaskCanceledException>(() =>
                myDispatcher.Invoke(() => _output.WriteLine("invoke 2")));

            _output.WriteLine("done.");
        }

        [Fact]
        public void RectTest()
        {
            var rect = new Rect(10, 20, 30, 40);
            Assert.Equal(Rect.Inflate(rect, -1, +1), rect.InflateValid(-1, +1));
            Assert.Equal(Rect.Inflate(rect, +2, -2), rect.InflateValid(+2, -2));

            var result1 = rect.InflateValid(-20, -10);
            Assert.Equal(0.0, result1.Width);
            Assert.Equal(20.0, result1.Height);
            Assert.Equal(25.0, result1.X);
            Assert.Equal(30.0, result1.Y);
        }

        [Fact]
        public void MathUtilityTest()
        {
            Assert.Equal(5, MathUtility.NormalizeLoopRange(0, 1, 5));
            Assert.Equal(1, MathUtility.NormalizeLoopRange(1, 1, 5));
            Assert.Equal(2, MathUtility.NormalizeLoopRange(2, 1, 5));
            Assert.Equal(3, MathUtility.NormalizeLoopRange(3, 1, 5));
            Assert.Equal(4, MathUtility.NormalizeLoopRange(4, 1, 5));
            Assert.Equal(5, MathUtility.NormalizeLoopRange(5, 1, 5));
            Assert.Equal(1, MathUtility.NormalizeLoopRange(6, 1, 5));
            Assert.Equal(2, MathUtility.NormalizeLoopRange(7, 1, 5));
            Assert.Equal(3, MathUtility.NormalizeLoopRange(8, 1, 5));
            Assert.Equal(4, MathUtility.NormalizeLoopRange(9, 1, 5));
            Assert.Equal(5, MathUtility.NormalizeLoopRange(10, 1, 5));
            Assert.Equal(1, MathUtility.NormalizeLoopRange(11, 1, 5));
            Assert.Equal(2, MathUtility.NormalizeLoopRange(12, 1, 5));

            Assert.Equal(4, MathUtility.NormalizeLoopRange(-1, 1, 5));
            Assert.Equal(3, MathUtility.NormalizeLoopRange(-2, 1, 5));
            Assert.Equal(2, MathUtility.NormalizeLoopRange(-3, 1, 5));
            Assert.Equal(1, MathUtility.NormalizeLoopRange(-4, 1, 5));
            Assert.Equal(5, MathUtility.NormalizeLoopRange(-5, 1, 5));
            Assert.Equal(4, MathUtility.NormalizeLoopRange(-6, 1, 5));
            Assert.Equal(3, MathUtility.NormalizeLoopRange(-7, 1, 5));
            Assert.Equal(2, MathUtility.NormalizeLoopRange(-8, 1, 5));
            Assert.Equal(1, MathUtility.NormalizeLoopRange(-9, 1, 5));
            Assert.Equal(5, MathUtility.NormalizeLoopRange(-10, 1, 5));
            Assert.Equal(4, MathUtility.NormalizeLoopRange(-11, 1, 5));
            Assert.Equal(3, MathUtility.NormalizeLoopRange(-12, 1, 5));
        }

        [Fact]
        public void MathUtilityCycleRangeTest()
        {
            Assert.Equal(-1, MathUtility.CycleLoopRange(0, 1, 3));
            Assert.Equal(0, MathUtility.CycleLoopRange(1, 1, 3));
            Assert.Equal(0, MathUtility.CycleLoopRange(2, 1, 3));
            Assert.Equal(0, MathUtility.CycleLoopRange(3, 1, 3));
            Assert.Equal(1, MathUtility.CycleLoopRange(4, 1, 3));
            Assert.Equal(1, MathUtility.CycleLoopRange(5, 1, 3));
            Assert.Equal(1, MathUtility.CycleLoopRange(6, 1, 3));
            Assert.Equal(2, MathUtility.CycleLoopRange(7, 1, 3));
            Assert.Equal(2, MathUtility.CycleLoopRange(8, 1, 3));
            Assert.Equal(2, MathUtility.CycleLoopRange(9, 1, 3));
            Assert.Equal(3, MathUtility.CycleLoopRange(10, 1, 3));

            Assert.Equal(-1, MathUtility.CycleLoopRange(-1, 1, 3));
            Assert.Equal(-1, MathUtility.CycleLoopRange(-2, 1, 3));
            Assert.Equal(-2, MathUtility.CycleLoopRange(-3, 1, 3));
            Assert.Equal(-2, MathUtility.CycleLoopRange(-4, 1, 3));
            Assert.Equal(-2, MathUtility.CycleLoopRange(-5, 1, 3));
            Assert.Equal(-3, MathUtility.CycleLoopRange(-6, 1, 3));
            Assert.Equal(-3, MathUtility.CycleLoopRange(-7, 1, 3));
            Assert.Equal(-3, MathUtility.CycleLoopRange(-8, 1, 3));
            Assert.Equal(-4, MathUtility.CycleLoopRange(-9, 1, 3));
            Assert.Equal(-4, MathUtility.CycleLoopRange(-10, 1, 3));
        }


        [Theory]
        [InlineData(0.0, AngleDirection.Forward)]
        [InlineData(90.0, AngleDirection.Right)]
        [InlineData(180.0, AngleDirection.Back)]
        [InlineData(270.0, AngleDirection.Left)]
        [InlineData(360.0, AngleDirection.Forward)]
        [InlineData(450.0, AngleDirection.Right)]
        [InlineData(-90.0, AngleDirection.Left)]
        [InlineData(-180.0, AngleDirection.Back)]
        [InlineData(-270.0, AngleDirection.Right)]
        [InlineData(-360.0, AngleDirection.Forward)]
        [InlineData(-450.0, AngleDirection.Left)]
        [InlineData(44.0, AngleDirection.Forward)]
        [InlineData(46.0, AngleDirection.Right)]
        [InlineData(-44.0, AngleDirection.Forward)]
        [InlineData(-46.0, AngleDirection.Left)]
        public void DegreeToDirectionTest(double degree, AngleDirection isRotate)
        {
            var result = MathUtility.DegreeToDirection(degree);
            Assert.Equal(isRotate, result);
            Assert.Equal(isRotate, result);
        }

        [Fact]
        public void DirectionExtensionsTest()
        {
            Assert.False(AngleDirection.Forward.IsHorizontal());
            Assert.True(AngleDirection.Forward.IsVertical());
            Assert.False(AngleDirection.Back.IsHorizontal());
            Assert.True(AngleDirection.Back.IsVertical());

            Assert.True(AngleDirection.Left.IsHorizontal());
            Assert.False(AngleDirection.Left.IsVertical());
            Assert.True(AngleDirection.Right.IsHorizontal());
            Assert.False(AngleDirection.Right.IsVertical());
        }

        [Fact]
        public void ShortcutKeyTest()
        {
            var l1a = new ShortcutKey("Ctrl+A,Shift+B");
            var l1b = new ShortcutKey("Ctrl+A,Shift+B");
            var r1a = new ShortcutKey("Ctrl+B,Shift+A");

#pragma warning disable CS1718 // 同じ変数と比較されました
            Assert.True(l1a == l1a);
#pragma warning restore CS1718 // 同じ変数と比較されました
            Assert.True(l1a == l1b);
            Assert.False(l1a == r1a);

            Assert.True(l1a.Equals(l1a));
            Assert.True(l1a.Equals(l1b));
            Assert.False(l1a.Equals(r1a));

            Assert.True(ReferenceEquals(l1a, l1a));
            Assert.False(ReferenceEquals(l1a, l1b));
            Assert.False(ReferenceEquals(l1a, r1a));
        }

        [Fact]
        public void MouseSequenceTest()
        {
            var l1a = new MouseSequence("LRL");
            var l1b = new MouseSequence("LRL");
            var r1a = new MouseSequence("LRLC");

#pragma warning disable CS1718 // 同じ変数と比較されました
            Assert.True(l1a == l1a);
#pragma warning restore CS1718 // 同じ変数と比較されました
            Assert.True(l1a == l1b);
            Assert.False(l1a == r1a);

            Assert.True(l1a.Equals(l1a));
            Assert.True(l1a.Equals(l1b));
            Assert.False(l1a.Equals(r1a));

            Assert.True(ReferenceEquals(l1a, l1a));
            Assert.False(ReferenceEquals(l1a, l1b));
            Assert.False(ReferenceEquals(l1a, r1a));
        }

        [Fact]
        public void TouchGestureTest()
        {
            var l1a = new TouchGesture("TouchL1");
            var l1b = new TouchGesture("TouchL1");
            var r1a = new TouchGesture("TouchR1");

#pragma warning disable CS1718 // 同じ変数と比較されました
            Assert.True(l1a == l1a);
#pragma warning restore CS1718 // 同じ変数と比較されました
            Assert.True(l1a == l1b);
            Assert.False(l1a == r1a);

            Assert.True(l1a.Equals(l1a));
            Assert.True(l1a.Equals(l1b));
            Assert.False(l1a.Equals(r1a));

            Assert.True(ReferenceEquals(l1a, l1a));
            Assert.False(ReferenceEquals(l1a, l1b));
            Assert.False(ReferenceEquals(l1a, r1a));
        }



        [Theory]
        [InlineData("aaa", "aaa")]
        [InlineData("a_a", "a:a")]
        [InlineData("\\aa\\bb", "\\aa\\bb")]
        [InlineData("\\aa\\bb", "/aa/bb")]
        [InlineData("\\aa\\bb\\", "/aa/bb/")]
        public void ValidPath(string expected, string actual)
        {
            var result = LoosePath.ValidPath(actual);
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(@"file:C:\Hoge\test.txt", @"C:\Hoge\test.txt")]
        [InlineData(@"file:\C:\Hoge\test.txt\", @"C:\Hoge\test.txt")]
        [InlineData(@"file:\\C:\Hoge\test.txt", @"C:\Hoge\test.txt")]
        [InlineData(@"file://C:/Hoge/test.txt", @"C:\Hoge\test.txt")]
        [InlineData(@"file:\\C:", @"C:\")]
        [InlineData(@"file:c:\", @"C:\")]
        [InlineData(@"\dir1\dir2", @"\dir1\dir2")]
        [InlineData(@"dir1\dir2", @"dir1\dir2")]
        [InlineData(@"file:\dir1\dir2", @"\dir1\dir2")]
        [InlineData(@"file:dir1\dir2", @"dir1\dir2")]
        [InlineData(@"\\host\any", @"\\host\any")]
        [InlineData(@"file:\\host\any\", @"\\host\any")]
        public void QueryFilePathTest(string source, string path)
        {
            var query = new QueryPath(QueryScheme.File, path);
            var actual = new QueryPath(source);

            Assert.Equal(query, actual);
            Assert.Equal(path, actual.Path);
            Assert.Equal(@"file:" + path, actual.FullPath);
            Assert.Equal(path, actual.SimplePath);
        }


        [Theory]
        [InlineData(@"\\Server\Share", @"\\Server\Share")]
        [InlineData(@"\\server\share\", @"\\Server\Share")]
        [InlineData(@"file:\\SERVER\share\", @"\\Server\Share")]
        [InlineData(@"\\server\SHARE\File.txt", @"\\Server\Share\File.txt")]
        public void QueryUncPathTest(string source, string path)
        {
            var query = new QueryPath(QueryScheme.File, path).Normalize();
            var actual = new QueryPath(source).Normalize();

            Assert.Equal(query, actual);
            Assert.Equal(path, actual.Path);
            Assert.Equal(@"file:" + path, actual.FullPath);
            Assert.Equal(path, actual.SimplePath);
        }


        [Theory]
        [InlineData(@"bookmark:folder\item1")]
        [InlineData(@"bookmark:\folder\item1\")]
        [InlineData(@"bookmark:\\folder\item1")]
        [InlineData(@"bookmark://folder/item1")]
        public void QueryBookmarkPathTest(string source)
        {
            var path = @"folder\item1";
            var query = new QueryPath(QueryScheme.Bookmark, path);
            var actual = new QueryPath(source);

            Assert.Equal(query, actual);
            Assert.Equal(path, actual.Path);
            Assert.Equal(@"bookmark:folder\item1", actual.FullPath);
            Assert.Equal(@"bookmark:folder\item1", actual.SimplePath);
        }
    }
}
