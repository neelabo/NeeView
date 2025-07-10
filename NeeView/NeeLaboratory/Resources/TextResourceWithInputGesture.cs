using NeeView;
using System;
using System.Windows.Input;

namespace NeeLaboratory.Resources
{
    public class TextResourceWithInputGesture : ITextResource
    {
        private readonly ITextResource _resource;

        public TextResourceWithInputGesture(ITextResource resource)
        {
            _resource = resource;
        }

        public TextResourceString? GetResourceString(string key)
        {
            return GetInputGestureResourceString(key) ?? _resource.GetResourceString(key);
        }

        public TextResourceString? GetCaseResourceString(string key, string pattern)
        {
            return GetInputGestureResourceString(key) ?? _resource.GetCaseResourceString(key, pattern);
        }

        private TextResourceString? GetInputGestureResourceString(string key)
        {
            var tokens = key.Split('.', 2);
            if (tokens.Length < 2)
            {
                return null;
            }

            return tokens[0] switch
            {
                nameof(Key)
                    => GetKeyString(tokens[1]),
                nameof(ModifierKeys)
                    => GetModifierKeysString(tokens[1]),
                _
                    => null
            };
        }

        private static TextResourceString? GetKeyString(string keyGesture)
        {
            return Enum.TryParse<Key>(keyGesture, out var inputKey) ? new TextResourceString(inputKey.GetDisplayString(), TextResourceStringAttribute.IsExpanded) : null;
        }

        private static TextResourceString? GetModifierKeysString(string keyGesture)
        {
            return Enum.TryParse<ModifierKeys>(keyGesture, out var modifierKey) ? new TextResourceString(modifierKey.GetDisplayString(), TextResourceStringAttribute.IsExpanded) : null;
        }
    }
}
