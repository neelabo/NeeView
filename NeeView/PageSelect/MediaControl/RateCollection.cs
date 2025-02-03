using NeeLaboratory.Generators;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class RateCollection : INotifyPropertyChanged
    {
        public List<double> _items = [2.0, 1.75, 1.5, 1.25, 1.0, 0.75, 0.5, 0.25];
        private double _selected = 1.0;

        public RateCollection()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public List<double> Rates => _items;

        public double Selected
        {
            get { return _selected; }
            set
            {
                SetProperty(ref _selected, value);
            }
        }

        public static string GetDisplayString(double rate, bool list)
        {
            if (rate == 1.0)
            {
                var normal = ResourceService.GetString("@Word.Normal");
                return list ? $"{rate:0.0#} ({normal})" : normal;
            }
            else if (rate <= 0.0)
            {
                return "";
            }
            else
            {
                return $"{rate:0.0#}";
            }
        }
    }
}

