using NeeView.Media.Imaging.Metadata;
using System;

namespace NeeView
{
    public class GpsLocation
    {
        ////public static string GoogleMapFormatA => @"https://www.google.com/maps/@$LatDeg,$LonDeg,15z";
        ////public static string GoogleMapFormatB => @"https://www.google.com/maps/place/$Lat+$Lon/";


        readonly ExifGpsDegree _latitude;
        readonly ExifGpsDegree _longitude;

        public GpsLocation(ExifGpsDegree latitude, ExifGpsDegree longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }

        public void OpenMap(string format)
        {
            if (format is null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            if (!_latitude.IsValid || !_longitude.IsValid) return;

            var s = format;
            s = s.Replace("$LatDeg", _latitude.ToValueString("{0:F5}"), StringComparison.Ordinal);
            s = s.Replace("$LonDeg", _longitude.ToValueString("{0:F5}"), StringComparison.Ordinal);
            s = s.Replace("$Lat", _latitude.ToFormatString(), StringComparison.Ordinal);
            s = s.Replace("$Lon", _longitude.ToFormatString(), StringComparison.Ordinal);

            ExternalProcess.Start(s);
        }
    }
}
