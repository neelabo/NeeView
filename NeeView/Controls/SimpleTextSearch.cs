using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Threading;

namespace NeeView
{
    public class SimpleTextSearch
    {
        private DispatcherTimer? _timeoutTimer;
        private string _prefix = "";
        private readonly List<string> _charsEntered = new(10);

        public SimpleTextSearch()
        {
        }

        private void AddCharToPrefix(string newChar)
        {
            if (_prefix.Length >= 64) return;
            _prefix += newChar;
            _charsEntered.Add(newChar);
        }

        private bool IsRepeatPrefix()
        {
            if (_charsEntered.Count == 0) return false;
            var c = _charsEntered[0];
            return _charsEntered.All(e => string.Compare(e, c, StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        public bool DoSearch(ITextSearchCollection collection, string nextChar)
        {
            AddCharToPrefix(nextChar);

            int startItemIndex = collection.SelectedIndex;

            int matchedItemIndex;
            if (IsRepeatPrefix())
            {
                matchedItemIndex = FindMatchingPrefix(collection, startItemIndex + 1, _charsEntered.First());
            }
            else
            {
                matchedItemIndex = FindMatchingPrefix(collection, startItemIndex, _prefix);
            }

            if (matchedItemIndex != -1)
            {
                collection.NavigateToItem(matchedItemIndex);
            }

            ResetTimeout();

            return (matchedItemIndex != -1);
        }

        private int FindMatchingPrefix(ITextSearchCollection collection, int startItemIndex, string prefix)
        {
            int count = collection.ItemsCount;
            if (count == 0)
            {
                return -1;
            }

            if (string.IsNullOrEmpty(prefix))
            {
                return -1;
            }

            startItemIndex = (startItemIndex + count) % count;

            int matchedItemIndex = -1;
            for (int currentIndex = startItemIndex; currentIndex < count;)
            {
                var itemString = collection.GetPrimaryText(currentIndex);

                if (itemString != null && itemString.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    matchedItemIndex = currentIndex;
                    break;
                }

                currentIndex++;
                if (currentIndex >= count)
                {
                    currentIndex = 0;
                }

                if (currentIndex == startItemIndex)
                {
                    break;
                }
            }

            return matchedItemIndex;
        }

        private void ResetTimeout()
        {
            if (_timeoutTimer == null)
            {
                _timeoutTimer = new DispatcherTimer(DispatcherPriority.Normal);
                _timeoutTimer.Tick += new EventHandler(OnTimeout);
            }
            else
            {
                _timeoutTimer.Stop();
            }

            _timeoutTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _timeoutTimer.Start();
        }

        private void OnTimeout(object? sender, EventArgs e)
        {
            ResetState();
        }

        public void ResetState()
        {
            _prefix = "";
            _charsEntered.Clear();

            _timeoutTimer?.Stop();
            _timeoutTimer = null;
        }
    }
}
