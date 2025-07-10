using NeeLaboratory.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NeeView
{
    public static class InputGestureDisplayString
    {
        public static List<string> ErrorMessages { get; private set; } = new();

        public static void Initialize(TextResourceManager resource)
        {
            var factory = new TextResourceSegmentFactory(resource.Map);

            InitializeKeyMap(factory);
            InitializeModifierKeysMap(factory);
            InitializeMouseButtonMap(factory);
            InitializeMouseActionMap(factory);
            InitializeTouchAreaMap(factory);
            InitializeMouseDirectionMap(factory);
        }

        private static InputGestureStringConverter GetDisplayStringStringConverter(TextResourceSegment map, string prefix)
        {
            var uppercase = map.TryGetValue(prefix + "._Uppercase", out var value) && value.Text.String.Equals("true", StringComparison.OrdinalIgnoreCase);
            return new InputGestureStringConverter(uppercase);
        }

        private static IEnumerable<KeyValuePair<string, TextResourceItem>> CollectTextItems(TextResourceSegment map, string prefix)
        {
            return map.Where(e => e.Key[prefix.Length + 1] != '_');
        }

        private static void InitializeKeyMap(TextResourceSegmentFactory factory)
        {
            var prefix = nameof(Key);
            var map = factory.Create(prefix);

            foreach (var pair in CollectTextItems(map, prefix))
            {
                var name = pair.Key.AsSpan(prefix.Length + 1);
                if (Enum.TryParse<Key>(name, true, out var key))
                {
                    key.SetDisplayString(pair.Value.Text.String);
                }
                else
                {
                    ErrorMessages.Add($"{prefix}.{name} is not valid.");
                }
            }

            KeyExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(map, prefix));
        }

        private static void InitializeModifierKeysMap(TextResourceSegmentFactory factory)
        {
            var prefix = nameof(ModifierKeys);
            var map = factory.Create(prefix);

            foreach (var pair in CollectTextItems(map, prefix))
            {
                var name = pair.Key.AsSpan(prefix.Length + 1);
                if (Enum.TryParse<ModifierKeys>(name, true, out var key))
                {
                    key.SetDisplayString(pair.Value.Text.String);
                }
                else
                {
                    ErrorMessages.Add($"{prefix}.{name} is not valid.");
                }
            }

            ModifierKeysExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(map, prefix));
        }

        private static void InitializeMouseButtonMap(TextResourceSegmentFactory factory)
        {
            var prefix = nameof(MouseButton);
            var map = factory.Create(prefix);

            foreach (var pair in CollectTextItems(map, prefix))
            {
                var name = pair.Key.AsSpan(prefix.Length + 1);
                if (Enum.TryParse<MouseButton>(name, true, out var key))
                {
                    key.SetDisplayString(pair.Value.Text.String);
                }
                else
                {
                    ErrorMessages.Add($"{prefix}.{name} is not valid.");
                }
            }

            MouseButtonExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(map, prefix));
        }

        private static void InitializeMouseActionMap(TextResourceSegmentFactory factory)
        {
            var prefix = nameof(MouseAction);
            var map = factory.Create(prefix);

            foreach (var pair in CollectTextItems(map, prefix))
            {
                var name = pair.Key.AsSpan(prefix.Length + 1);
                if (Enum.TryParse<MouseAction>(name, true, out var key))
                {
                    key.SetDisplayString(pair.Value.Text.String);
                }
                else
                {
                    ErrorMessages.Add($"{prefix}.{name} is not valid.");
                }
            }

            MouseActionExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(map, prefix));
        }

        private static void InitializeTouchAreaMap(TextResourceSegmentFactory factory)
        {
            var prefix = nameof(TouchArea);
            var map = factory.Create(prefix);

            foreach (var pair in CollectTextItems(map, prefix))
            {
                var name = pair.Key.AsSpan(prefix.Length + 1);
                if (Enum.TryParse<TouchArea>(name, true, out var key))
                {
                    key.SetDisplayString(pair.Value.Text.String);
                }
                else
                {
                    ErrorMessages.Add($"{prefix}.{name} is not valid.");
                }
            }

            TouchAreaExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(map, prefix));
        }

        private static void InitializeMouseDirectionMap(TextResourceSegmentFactory factory)
        {
            var prefix = nameof(MouseDirection);
            var map = factory.Create(prefix);

            foreach (var pair in CollectTextItems(map, prefix))
            {
                var name = pair.Key.AsSpan(prefix.Length + 1);
                if (Enum.TryParse<MouseDirection>(name, true, out var key))
                {
                    key.SetDisplayString(pair.Value.Text.String);
                }
                else
                {
                    ErrorMessages.Add($"{prefix}.{name} is not valid.");
                }
            }

            MouseGestureDirectionExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(map, prefix));
        }

        public static string GetDisplayString(InputGesture gesture)
        {
            return InputGestureConverter.GetDisplayString(gesture);
        }
    }


    public class InputGestureStringConverter : StringConverter
    {
        private readonly bool _isUppercase;

        public InputGestureStringConverter(bool isUppercase)
        {
            _isUppercase = isUppercase;
        }

        public override string Convert(string value)
        {
            return _isUppercase ? value.ToUpperInvariant() : value;
        }
    }
}
