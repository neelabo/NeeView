using System;
using System.Diagnostics;
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

        public DragActionMemento? DefaultMemento { get; private set; }


        public abstract DragActionControl CreateControl(DragTransformContext context);


        public void CreateDefaultMemento()
        {
            SetDefaultMemento(CreateMemento());
        }

        [Conditional("DEBUG")]
        public void CheckDefaultMemento()
        {
            Debug.Assert(DefaultMemento is not null);
        }

        public void SetDefaultMemento(DragActionMemento memento)
        {
#if DEBUG
            Debug.Assert(memento != null);
            Debug.Assert(memento.MouseButton != null);
            if (ParameterSource is not null)
            {
                Debug.Assert(memento.Parameter != null && memento.Parameter.Equals(ParameterSource.GetDefault()));
            }

#endif
            DefaultMemento = memento;
        }


        #region Memento

        public DragActionMemento CreateMemento(bool trim = false)
        {
            var memento = new DragActionMemento();
            memento.MouseButton = DragKey;
            memento.Parameter = (DragActionParameter?)Parameter?.Clone();

            if (trim)
            {
                Debug.Assert(DefaultMemento is not null);
                if (memento.MouseButton == DefaultMemento.MouseButton)
                {
                    memento.MouseButton = null;
                }
                if (memento.Parameter is not null && memento.Parameter.Equals(DefaultMemento.Parameter))
                {
                    memento.Parameter = null;
                }
            }

            return memento;
        }

        public void Restore(DragActionMemento memento)
        {
            Debug.Assert(DefaultMemento is not null);

            if (memento == null) return;

            DragKey = memento.MouseButton ?? DefaultMemento.MouseButton ?? throw new InvalidOperationException();
            Parameter = (DragActionParameter?)memento.Parameter?.Clone() ?? DefaultMemento.Parameter;
        }

        #endregion
    }


    public class DragActionMemento
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DragKey? MouseButton { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DragActionParameter? Parameter { get; set; }


        public DragActionMemento Clone()
        {
            return (DragActionMemento)MemberwiseClone();
        }

        public bool IsDefault()
        {
            return MouseButton == null && Parameter == null;
        }

        //public bool MemberwiseEquals(DragActionMemento other)
        //{
        //    if (other is null) return false;
        //    if (other.MouseButton != MouseButton) return false;
        //    if (Parameter != null && !Parameter.Equals(other.Parameter)) return false;
        //    return true;
        //}
    }

}
