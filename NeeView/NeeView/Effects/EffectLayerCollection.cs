using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView
{
    public class EffectLayerCollection : ObservableCollection<EffectLayer>, IEquatable<EffectLayerCollection>
    {
        private const int MaxLayerCount = 10;

        #region IEquatable

        // NOTE: DiffJsonConverter 用。限定的な条件でのみ成立
        public bool Equals(EffectLayerCollection? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // 等しい数の空レイヤーのみの場合に等価
            return other.GetType() == this.GetType()
                && other.Count == this.Count
                && other.All(e => e.Effect is null&& e.IsEnabled) && this.All(e => e.Effect is null && e.IsEnabled);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as EffectLayerCollection);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            return hashCode.ToHashCode();
        }

        #endregion  IEquatable

        public bool CanCreateNew()
        {
            return this.Count < MaxLayerCount;
        }

        public bool CanMoveUp(EffectLayer layer)
        {
            if (!this.Contains(layer)) return false;

            var index = this.IndexOf(layer);
            return index > 0;
        }

        public bool CanMoveDown(EffectLayer layer)
        {
            if (!this.Contains(layer)) return false;

            var index = this.IndexOf(layer);
            return index < this.Count - 1;
        }

        public bool CanDelete(EffectLayer layer)
        {
            if (!this.Contains(layer)) return false;

            return this.Count > 1;
        }

        public EffectLayer? CreateNew()
        {
            if (!CanCreateNew()) return null;

            var layer = new EffectLayer();
            this.Insert(0, layer);
            return layer;
        }

        public void MoveUp(EffectLayer layer)
        {
            if (!CanMoveUp(layer)) return;

            var index = this.IndexOf(layer);
            this.Move(index, index - 1);
        }

        public void MoveDown(EffectLayer layer)
        {
            if (!CanMoveDown(layer)) return;

            var index = this.IndexOf(layer);
            this.Move(index, index + 1);
        }

        public void Delete(EffectLayer layer)
        {
            if (!CanDelete(layer)) return;

            this.Remove(layer);
            Config.Current.ImageEffect.Caches.Add(layer.Effect);
        }
    }
}