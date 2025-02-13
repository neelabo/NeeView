using System.Collections.Generic;
using System.ComponentModel;

namespace NeeLaboratory.ComponentModel
{
    /// <summary>
    /// PropertyChanged イベント処理
    /// </summary>
    /// <remarks>
    /// 未テスト
    /// </remarks>
    public class PropertyChangedEventReceiver
    {
        private Dictionary<string, PropertyChangedEventHandler> _map = new();

        public void AddListener(string propertyName, PropertyChangedEventHandler handler)
        {
            _map.Add(propertyName, handler);
        }

        public void RemoveListener(string propertyName)
        {
            _map.Remove(propertyName);
        }

        public void Clear()
        {
            _map.Clear();
        }

        public void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
            {
                foreach (var handler in _map.Values)
                {
                    handler(sender, e);
                }
            }
            else
            {
                if (_map.TryGetValue(e.PropertyName, out PropertyChangedEventHandler? handler))
                {
                    handler(sender, e);
                }
            }
        }
    }

}
