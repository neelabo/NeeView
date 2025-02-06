using NeeLaboratory.Generators;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    public static class MediaRateTools
    {
        public static List<double> Rates { get; } = [2.0, 1.75, 1.5, 1.25, 1.0, 0.75, 0.5, 0.25];

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

