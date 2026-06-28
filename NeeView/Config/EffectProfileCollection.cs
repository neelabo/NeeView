using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeView.Collections.ObjectModel;
using System;
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
        private static Lazy<EffectProfileCollection> _current = new();
        public static EffectProfileCollection Current => _current.Value;


        private readonly EffectProfileCollectionConfig _config;
        private readonly BookSettingConfig _bookSetting;
        private EffectProfile? _selectedProfile;
        private bool _isSuspend;


        public EffectProfileCollection()
        {
            _config = Config.Current.EffectProfiles;
            _config.Profiles.SubscribeCollectionChanged(OnProfilesCollectionChanged);

            _bookSetting = Config.Current.BookSetting;
            _bookSetting.SubscribePropertyChanged(nameof(_bookSetting.EffectProfileId), OnSelectedIndexChanged);
        }


        public ObservableCollectionEx<EffectProfile> Profiles => _config.Profiles;

        public EffectProfile? SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (_selectedProfile != value)
                {
                    OnSelectedProfileChanging(_selectedProfile, value);
                    _selectedProfile = value;
                    OnSelectedProfileChanged(_selectedProfile);
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedId
        {
            get => _bookSetting.EffectProfileId;
            set => _bookSetting.EffectProfileId = value;
        }


        private void OnSelectedProfileChanging(EffectProfile? oldValue, EffectProfile? newValue)
        {
            if (_isSuspend) return;

            oldValue?.Store(Config.Current);
        }

        private void OnSelectedProfileChanged(EffectProfile? value)
        {
            if (_isSuspend) return;

            if (value is null) return;
            value.Restore(Config.Current);

            SelectedId = value.Id;
        }

        private void OnSelectedIndexChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isSuspend) return;

            AppDispatcher.Invoke(() => ResolveSelectedProfile());
        }

        private void OnProfilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isSuspend) return;

            AppDispatcher.Invoke(() => ResolveSelectedProfile());
        }

        public IDisposable Suspend()
        {
            _isSuspend = true;
            var selectedId = SelectedId;

            return new AnonymousDisposable(() =>
            {
                SelectedId = selectedId;
                _isSuspend = false;
            });
        }

        public void Restore()
        {
            Debug.Assert(!_isSuspend);

            _selectedProfile = null;
            ResolveSelectedProfile();
        }

        public void Store()
        {
            SelectedProfile?.Store(Config.Current);
        }

        [MemberNotNull(nameof(SelectedProfile))]
        public void ResolveSelectedProfile()
        {
            var profile = Profiles.FirstOrDefault(p => p.Id == SelectedId);
            if (profile is not null)
            {
                SelectedProfile = profile;
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
            var profile = new EffectProfile(id) { Name = CreateUniqueName(SelectedProfile?.DisplayName) };
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
            return Profiles.Any(e => e.DisplayName.Equals(name, StringComparison.Ordinal));
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

        public void SetSelectedId(int id)
        {
            if (!Profiles.Any(e => e.Id == id)) return;

            SelectedId = id;
        }

        public EffectProfile? GetNext(int offset)
        {
            Debug.Assert(-1 <= offset && offset <= 1);

            var current = SelectedProfile;
            if (current is null) return null;

            var index = Profiles.IndexOf(current);
            var next = (index + offset + Profiles.Count) % Profiles.Count;
            return Profiles[next];
        }
    }
}
