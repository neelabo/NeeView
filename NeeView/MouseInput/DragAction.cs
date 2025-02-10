using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NeeView
{
    public abstract partial class DragAction
    {
        [GeneratedRegex(@"DragAction$")]
        private static partial Regex _termDragActionRegex { get; }


        public delegate DragActionControl CreateDragAction(DragTransformContext context, DragAction? source);

        public DragAction()
        {
            Name = _termDragActionRegex.Replace(GetType().Name, "");
        }

        public DragAction(string name)
        {
            Name = name;
        }


        public string Name { get; }

        public string Note { get; init; } = "";

        public bool IsDummy { get; init; }

        public bool IsLocked { get; init; }

        public DragKey DragKey { get; set; } = DragKey.Empty;

        public DragActionParameterSource? ParameterSource { get; init; }

        public DragActionParameter? Parameter
        {
            get => ParameterSource?.Get();
            set => ParameterSource?.Set(value);
        }

        public DragActionCategory DragActionCategory { get; protected set; }


        public abstract DragActionControl CreateControl(DragTransformContext context);


        #region Memento

        public class Memento
        {
            public DragKey MouseButton { get; set; } = DragKey.Empty;

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public DragActionParameter? Parameter { get; set; }


            public Memento Clone()
            {
                return (Memento)MemberwiseClone();
            }
            
            public bool MemberwiseEquals(Memento other)
            {
                if (other is null) return false;
                if (other.MouseButton != MouseButton) return false;
                if (Parameter != null && !Parameter.MemberwiseEquals(other.Parameter)) return false;
                return true;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.MouseButton = DragKey;
            memento.Parameter = (DragActionParameter?)Parameter?.Clone();
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            DragKey = memento.MouseButton;
            Parameter = (DragActionParameter?)memento.Parameter?.Clone();
        }

        #endregion
    }

}
