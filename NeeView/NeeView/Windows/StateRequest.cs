namespace NeeView.Windows
{
    public enum StateRequest
    {
        Disable,
        Enable,
        Toggle,
    }

    public static class StateRequestExtensions
    {
        public static bool ToIsEnabled(this StateRequest request, bool current)
        {
            return request switch
            {
                StateRequest.Enable => true,
                StateRequest.Disable => false,
                StateRequest.Toggle => !current,
                _ => current,
            };
        }

        public static StateRequest ToStateRequest(this bool isEnabled)
        {
            return isEnabled ? StateRequest.Enable : StateRequest.Disable;
        }
    }

}
