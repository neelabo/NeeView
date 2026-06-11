#define LOCAL_DEBUG

using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Properties;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NeeView
{
    [LocalDebug]
    public partial class EffectProfile : ObservableObject
    {
        public static string DefaultName => TextResources.GetString("Word.Profile") + " 1";

        public EffectProfile()
        {
        }

        public EffectProfile(int id)
        {
            Id = id;
        }

        public int Id { get; set; } = 0;

        [ObservableProperty]
        public partial string Name { get; set; } = "Default";

        public ImageCustomSizeConfig ImageCustomSize { get; set; } = new();
        public ImageTrimConfig ImageTrim { get; set; } = new();
        public ImageDotKeepConfig ImageDotKeep { get; set; } = new();
        public ImageResizeFilterConfig ImageResizeFilter { get; set; } = new();
        public ImageGridConfig ImageGrid { get; set; } = new();
        public ImageEffectConfig ImageEffect { get; set; } = new();


        public void Store(Config config)
        {
            LocalDebug.WriteLine(ToString());

            ObjectMerge.Merge(ImageCustomSize, config.ImageCustomSize);
            ObjectMerge.Merge(ImageTrim, config.ImageTrim);
            ObjectMerge.Merge(ImageDotKeep, config.ImageDotKeep);
            ObjectMerge.Merge(ImageResizeFilter, config.ImageResizeFilter);
            ObjectMerge.Merge(ImageGrid, config.ImageGrid);
            ObjectMerge.Merge(ImageEffect, config.ImageEffect);
        }

        public void Restore(Config config)
        {
            LocalDebug.WriteLine(ToString());

            ObjectMerge.Merge(config.ImageCustomSize, ImageCustomSize);
            ObjectMerge.Merge(config.ImageTrim, ImageTrim);
            ObjectMerge.Merge(config.ImageDotKeep, ImageDotKeep);
            ObjectMerge.Merge(config.ImageResizeFilter, ImageResizeFilter);
            ObjectMerge.Merge(config.ImageGrid, ImageGrid);
            ObjectMerge.Merge(config.ImageEffect, ImageEffect);
        }

        public bool ValueEquals(EffectProfile? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == this.GetType()
                && Equals(this.Id, other.Id)
                && Equals(this.Name, other.Name)
                && Equals(this.ImageCustomSize, other.ImageCustomSize)
                && Equals(this.ImageTrim, other.ImageTrim)
                && Equals(this.ImageDotKeep, other.ImageDotKeep)
                && Equals(this.ImageResizeFilter, other.ImageResizeFilter)
                && Equals(this.ImageGrid, other.ImageGrid)
                && Equals(this.ImageEffect, other.ImageEffect)
                ;
        }

        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
    }


    public class EffectProfileComparer : IEqualityComparer<EffectProfile>
    {
        public bool Equals(EffectProfile? x, EffectProfile? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return x.ValueEquals(y);
        }

        public int GetHashCode([DisallowNull] EffectProfile obj)
        {
            var hashCode = new global::System.HashCode();

            hashCode.Add(obj.GetType());
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.ImageCustomSize);
            hashCode.Add(obj.ImageTrim);
            hashCode.Add(obj.ImageDotKeep);
            hashCode.Add(obj.ImageResizeFilter);
            hashCode.Add(obj.ImageGrid);
            hashCode.Add(obj.ImageEffect);
            return hashCode.ToHashCode();
        }
    }

}
