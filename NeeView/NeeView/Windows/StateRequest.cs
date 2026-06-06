using System;

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

        public static StateRequest ToStateRequest(this ToggleMode toggleMode)
        {
            return toggleMode switch
            {
                ToggleMode.Toggle => StateRequest.Toggle,
                ToggleMode.On => StateRequest.Enable,
                ToggleMode.Off => StateRequest.Disable,
                _ => throw new ArgumentOutOfRangeException(nameof(toggleMode)),
            };
        }
    }

}
