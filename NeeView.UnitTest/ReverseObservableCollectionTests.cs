using NeeLaboratory;
using NeeView.Collections.ObjectModel;
using System;
using System.Collections.Specialized;
using Xunit.Abstractions;

namespace NeeView.UnitTest
{


    public class ReverseObservableCollectionTests
    {
        public class SampleReverseObservableCollection
        {
            private ObservableCollectionEx<int> _observableCollection;
            private ReverseObservableCollection<int> _reverseCollection;

            public SampleReverseObservableCollection()
            {
                _observableCollection = new ObservableCollectionEx<int> { 0, 1, 2, 3, 4 };
                _reverseCollection = new ReverseObservableCollection<int>(_observableCollection);
            }

            public ObservableCollectionEx<int> Items => _observableCollection;
            public ReverseObservableCollection<int> ReverseItems => _reverseCollection;
        }

        private readonly ITestOutputHelper _output;

        public ReverseObservableCollectionTests(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
        }

        [Fact]
        public void IndexTest()
        {
            var s = new SampleReverseObservableCollection();
            var items = s.ReverseItems.ToList();
            Assert.Equal([4, 3, 2, 1, 0], items);
        }

        [Theory]
        [InlineData(5)]
        public void AddEventTest(int n)
        {
            var s = new SampleReverseObservableCollection();

            s.Items.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
                Assert.Equal([n], e.NewItems?.Cast<int>());
                Assert.Equal(5, e.NewStartingIndex);
                Assert.Null(e.OldItems);
                Assert.Equal(-1, e.OldStartingIndex);
            };

            s.ReverseItems.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
                Assert.Equal([n], e.NewItems?.Cast<int>());
                Assert.Equal(0, e.NewStartingIndex);
                Assert.Null(e.OldItems);
                Assert.Equal(-1, e.OldStartingIndex);
            };

            s.Items.Add(n);
            Assert.Equal(s.Items.Reverse(), s.ReverseItems);
        }

        [Theory]
        [InlineData(0, 5, 5)]
        [InlineData(1, 4, 5)]
        [InlineData(2, 3, 5)]
        [InlineData(3, 2, 5)]
        [InlineData(4, 1, 5)]
        [InlineData(5, 0, 5)]
        public void InsertEventTest(int index, int reverseIndex, int n)
        {
            var s = new SampleReverseObservableCollection();

            s.Items.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
                Assert.Equal([n], e.NewItems?.Cast<int>());
                Assert.Equal(index, e.NewStartingIndex);
                Assert.Null(e.OldItems);
                Assert.Equal(-1, e.OldStartingIndex);
            };

            s.ReverseItems.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
                Assert.Equal([n], e.NewItems?.Cast<int>());
                Assert.Equal(reverseIndex, e.NewStartingIndex);
                Assert.Null(e.OldItems);
                Assert.Equal(-1, e.OldStartingIndex);
            };

            s.Items.Insert(index, n);
            Assert.Equal(s.Items.Reverse(), s.ReverseItems);
        }

        [Theory]
        [InlineData(0, 4, 5)]
        [InlineData(1, 3, 5)]
        [InlineData(2, 2, 5)]
        [InlineData(3, 1, 5)]
        [InlineData(4, 0, 5)]
        public void ReplaceEventTest(int index, int reverseIndex, int n)
        {
            var s = new SampleReverseObservableCollection();
            var oldNum = s.Items[index];

            s.Items.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Replace, e.Action);
                Assert.Equal([n], e.NewItems?.Cast<int>());
                Assert.Equal(index, e.NewStartingIndex);
                Assert.Equal([oldNum], e.OldItems?.Cast<int>());
                Assert.Equal(index, e.OldStartingIndex);
            };

            s.ReverseItems.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Replace, e.Action);
                Assert.Equal([n], e.NewItems?.Cast<int>());
                Assert.Equal(reverseIndex, e.NewStartingIndex);
                Assert.Equal([oldNum], e.OldItems?.Cast<int>());
                Assert.Equal(reverseIndex, e.OldStartingIndex);
            };

            s.Items[index] = n;
            Assert.Equal(s.Items.Reverse(), s.ReverseItems);
        }


        [Theory]
        [InlineData(0, 0, 4, 1, 2, 3, 4)]
        [InlineData(1, 1, 3, 0, 2, 3, 4)]
        [InlineData(2, 2, 2, 0, 1, 3, 4)]
        [InlineData(3, 3, 1, 0, 1, 2, 4)]
        [InlineData(4, 4, 0, 0, 1, 2, 3)]
        public void RemoveEventTest(int n, int index, int reverseIndex, params int[] items)
        {
            var s = new SampleReverseObservableCollection();

            s.Items.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action);
                Assert.Null(e.NewItems);
                Assert.Equal(-1, e.NewStartingIndex);
                Assert.Equal([n], e.OldItems?.Cast<int>());
                Assert.Equal(index, e.OldStartingIndex);
            };

            s.ReverseItems.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action);
                Assert.Null(e.NewItems);
                Assert.Equal(-1, e.NewStartingIndex);
                Assert.Equal([n], e.OldItems?.Cast<int>());
                Assert.Equal(reverseIndex, e.OldStartingIndex);
            };

            Assert.Equal(n, s.ReverseItems[reverseIndex]);
            s.Items.Remove(n);
            Assert.Equal(items, s.Items);
            Assert.Equal(s.Items.Reverse(), s.ReverseItems);
        }

        [Theory]
        [InlineData(0, 4, 4, 0)]
        [InlineData(1, 3, 3, 1)]
        [InlineData(2, 2, 2, 2)]
        [InlineData(3, 1, 1, 3)]
        [InlineData(4, 0, 0, 4)]
        [InlineData(1, 4, 3, 0)]
        [InlineData(2, 4, 2, 0)]
        [InlineData(3, 4, 1, 0)]
        public void MoveEventTest(int oldIndex, int newIndex, int oldIndexReverse, int newIndexReverse)
        {
            var s = new SampleReverseObservableCollection();

            var n = s.Items[oldIndex];

            s.ReverseItems.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Move, e.Action);
                Assert.Equal([n], e.NewItems?.Cast<int>());
                Assert.Equal(newIndexReverse, e.NewStartingIndex);
                Assert.Equal([n], e.OldItems?.Cast<int>());
                Assert.Equal(oldIndexReverse, e.OldStartingIndex);
            };

            s.Items.Move(oldIndex, newIndex);
            Assert.Equal(s.Items.Reverse(), s.ReverseItems);
        }

        [Theory]
        [InlineData(1, 2)]
        public void ResetEventTest(params int[] items)
        {
            var s = new SampleReverseObservableCollection();

            s.ReverseItems.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
                Assert.Null(e.NewItems);
                Assert.Equal(-1, e.NewStartingIndex);
                Assert.Null(e.OldItems);
                Assert.Equal(-1, e.OldStartingIndex);
            };

            s.Items.Reset(items);
            Assert.Equal(items, s.Items);
            Assert.Equal(s.Items.Reverse(), s.ReverseItems);
        }
    }

}
