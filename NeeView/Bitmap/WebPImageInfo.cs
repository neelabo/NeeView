namespace NeeView
{
    public class WebPImageInfo : ImageInfo
    {
        public WebPImageInfo()
        {
            DpiX = 72.0; // Microsoft の WebP コーデック値を継承
            DpiY = 72.0;
        }
    }

}
