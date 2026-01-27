using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace NeeView.Setting
{
    /// <summary>
    /// MouseGestureSetting ViewModel
    /// </summary>
    public class InputTouchSettingViewModel : BindableBase
    {
        private readonly IReadOnlyDictionary<string, CommandElement> _commandMap;
        private readonly string _key;
        private ObservableCollection<GestureElement> _gestureToken = new();
        private string? _gestureTokenNote;


        public InputTouchSettingViewModel(IReadOnlyDictionary<string, CommandElement> commandMap, string key, FrameworkElement gestureSender)
        {
            _commandMap = commandMap;
            _key = key;

            this.TouchAreaMap = new TouchAreaMap(_commandMap[_key].TouchGesture.Areas);
            UpdateGestureToken(this.TouchAreaMap);
        }


        public ObservableCollection<GestureElement> GestureToken
        {
            get { return _gestureToken; }
            set { if (_gestureToken != value) { _gestureToken = value; RaisePropertyChanged(); } }
        }

        public string? GestureTokenNote
        {
            get { return _gestureTokenNote; }
            set { if (_gestureTokenNote != value) { _gestureTokenNote = value; RaisePropertyChanged(); } }
        }

        public TouchAreaMap TouchAreaMap { get; set; }


        internal void SetTouchGesture(Point pos, double width, double height)
        {
            var area = TouchAreaExtensions.GetTouchArea(pos.X / width, pos.Y / height);

            this.TouchAreaMap.Toggle(area);
            RaisePropertyChanged(nameof(TouchAreaMap));

            UpdateGestureToken(this.TouchAreaMap);
        }

        /// <summary>
        /// Update Gesture Information
        /// </summary>
        /// <param name="map"></param>
        public void UpdateGestureToken(TouchAreaMap map)
        {
            var areas = map.ToAreas();
            this.GestureTokenNote = null;

            if (areas.Count > 0)
            {
                var shortcuts = new ObservableCollection<GestureElement>();
                foreach (var area in areas)
                {
                    var overlaps = _commandMap
                        .Where(i => i.Key != _key && i.Value.TouchGesture.Areas.Contains(area))
                        .Select(e => CommandTable.Current.GetElement(e.Key).LongText)
                        .ToList();

                    if (overlaps.Count > 0)
                    {
                        if (this.GestureTokenNote != null) this.GestureTokenNote += "\n";
                        this.GestureTokenNote += string.Format(CultureInfo.InvariantCulture, TextResources.GetString("Notice.ConflictWith"), area.GetDisplayString(), TextResources.Join(overlaps));
                    }

                    var element = new GestureElement();
                    element.Gesture = area.GetDisplayString();
                    element.IsConflict = overlaps.Count > 0;
                    element.Splitter = ",";

                    shortcuts.Add(element);
                }

                if (shortcuts.Count > 0)
                {
                    shortcuts.Last().Splitter = null;
                }

                this.GestureToken = shortcuts;
            }
            else
            {
                this.GestureToken = new ObservableCollection<GestureElement>();
            }
        }

        /// <summary>
        /// Decide
        /// </summary>
        public void Flush()
        {
            _commandMap[_key].TouchGesture = new TouchGesture(this.TouchAreaMap.ToAreas());
        }
    }


    /// <summary>
    /// タッチエリア管理用
    /// </summary>
    public class TouchAreaMap
    {
        private readonly Dictionary<TouchArea, bool> _map;

        public TouchAreaMap(List<TouchArea> areas)
        {
            _map = Enum.GetValues(typeof(TouchArea)).Cast<TouchArea>().ToDictionary(e => e, e => false);

            foreach (var area in areas)
            {
                _map[area] = true;
            }
        }

        public bool this[TouchArea area]
        {
            get { return _map[area]; }
            set { _map[area] = value; }
        }

        public void Toggle(TouchArea area)
        {
            _map[area] = !_map[area];
        }

        public void Clear()
        {
            foreach (var key in _map.Keys)
            {
                _map[key] = false;
            }
        }

        public List<TouchArea> ToAreas()
        {
            return _map.Where(e => e.Key != TouchArea.None && e.Value == true).Select(e => e.Key).ToList();
        }

        public override string ToString()
        {
            return string.Join(",", _map.Where(e => e.Key != TouchArea.None && e.Value == true).Select(e => e.Key.ToString()));
        }
    }

}
