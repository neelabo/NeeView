using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeView.Collections.ObjectModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeeView
{
    public partial class EffectProfileCollection : ObservableObject
    {
        private static Lazy<EffectProfileCollection> _current = new(() => new(Config.Current.EffectProfiles));
        public static EffectProfileCollection Current => _current.Value;


        private readonly EffectProfileCollectionConfig _config;


        public EffectProfileCollection(EffectProfileCollectionConfig config)
        {
            _config = config;
            _config.SubscribePropertyChanged(nameof(_config.SelectedId), OnSelectedIndexChanged);
            _config.Profiles.SubscribeCollectionChanged(OnProfilesCollectionChanged);
        }


        public ObservableCollection<EffectProfile> Profiles => _config.Profiles;

        [ObservableProperty]
        public partial EffectProfile? SelectedProfile { get; set; }


        partial void OnSelectedProfileChanging(EffectProfile? oldValue, EffectProfile? newValue)
        {
            oldValue?.Store(Config.Current);
        }

        partial void OnSelectedProfileChanged(EffectProfile? value)
        {
            if (value is null) return;
            value.Restore(Config.Current);
          
            _config.SelectedId = value.Id;
        }

        private void OnSelectedIndexChanged(object? sender, PropertyChangedEventArgs e)
        {
            ResolveSelectedProfile();
        }

        private void OnProfilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ResolveSelectedProfile();
        }

        public void Store()
        {
            SelectedProfile?.Store(Config.Current);
        }

        public void Restore()
        {
            Store();

            ResolveSelectedProfile();
            SelectedProfile.Restore(Config.Current);
        }

        [MemberNotNull(nameof(SelectedProfile))]
        public void ResolveSelectedProfile()
        {
            var profile = Profiles.FirstOrDefault(p => p.Id == _config.SelectedId);
            if (profile is not null)
            {
                SelectedProfile = profile;
                return;
            }

            if (SelectedProfile is not null)
            {
                return;
            }

            profile = Profiles.FirstOrDefault();
            if (profile is not null)
            {
                SelectedProfile = profile;
                return;
            }

            profile = new EffectProfile();
            Profiles.Add(profile);
            SelectedProfile = profile;
        }


        public void CreateNew()
        {
            var id = CreateUniqueId();
            var profile = new EffectProfile(id) { Name = CreateUniqueName() };
            Profiles.Add(profile);
            SelectedProfile = profile;
            _config.IdCounter = id % 0xFFFF;
        }

        public void Clone()
        {
            var id = CreateUniqueId();
            var profile = new EffectProfile(id) { Name = CreateUniqueName(SelectedProfile?.Name) };
            profile.Store(Config.Current);
            Profiles.Add(profile);
            SelectedProfile = profile;
            _config.IdCounter = id % 0xFFFF;
        }

        private int CreateUniqueId()
        {
            int id = _config.IdCounter;
            do
            {
                id++;
            }
            while (Profiles.Any(e => e.Id == id));
            return id;
        }

        private string CreateUniqueName(string? name = null)
        {
            name ??= EffectProfile.DefaultName;

            var names = Profiles.Select(e => e.Name).ToList();
            if (!ContainsName(name))
            {
                return name;
            }

            var body = name;
            var count = 1;
            var regex = new Regex(@"^(.+) (\d+)$");
            var match = regex.Match(name);
            if (match.Success)
            {
                body = match.Groups[1].Value;
                count = int.Parse(match.Groups[2].Value);
            }

            do
            {
                name = $"{body} {++count}";
            }
            while (ContainsName(name));

            return name;
        }

        private bool ContainsName(string name)
        {
            return Profiles.Any(e => e.Name.Equals(name, StringComparison.Ordinal));
        }

        public bool CanDelete(EffectProfile? profile)
        {
            if (profile is null) return false;

            return profile.Id != 0 && Profiles.Count > 1;
        }

        public void Delete(EffectProfile? profile)
        {
            if (profile is null) return;
            if (!CanDelete(profile)) return;

            if (SelectedProfile == profile)
            {
                var index = Profiles.IndexOf(profile);
                var next = Math.Clamp(index < Profiles.Count - 1 ? index + 1 : index - 1, 0, Profiles.Count - 1);
                SelectedProfile = Profiles[next];
            }

            Profiles.Remove(profile);
        }

        public bool CanRename(EffectProfile? profile)
        {
            if (profile is null) return false;

            return profile.Id != 0;
        }

        public void Rename(EffectProfile? profile, string name)
        {
            if (profile is null) return;
            if (!CanRename(profile)) return;

            profile.Name = CreateUniqueName(name);
        }
    }
}
