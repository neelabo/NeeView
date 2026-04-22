namespace NeeView.Windows
{
    public enum VisibilityRequest
    {
        Hidden,
        Visible,
        Toggle,
    }

    public static class VisibilityRequestExtensions
    {
        public static bool ToIsVisible(this VisibilityRequest request, bool current)
        {
            return request switch
            {
                VisibilityRequest.Visible => true,
                VisibilityRequest.Hidden => false,
                VisibilityRequest.Toggle => !current,
                _ => current,
            };
        }

        public static VisibilityRequest ToVisibilityRequest(this bool isVisible)
        {
            return isVisible ? VisibilityRequest.Visible : VisibilityRequest.Hidden;
        }
    }

}
