using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [JsonConverter(typeof(JsonFormatVersionConverter))]
    public record class FormatVersion : IComparable<FormatVersion>
    {
        public FormatVersion(string name)
        {
            Name = name;
            MajorVersion = Environment.AssemblyVersion.Major;
            MinorVersion = Environment.AssemblyVersion.Minor;
            BuildVersion = Environment.AssemblyVersion.Build;
        }

        public FormatVersion(string name, int majorVersion, int minorVersion, int buildVersion)
        {
            Name = name;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            BuildVersion = buildVersion;
        }

        public FormatVersion(string name, VersionNumber version)
        {
            Name = name;
            MajorVersion = version.MajorVersion;
            MinorVersion = version.MinorVersion;
            BuildVersion = version.BuildVersion;
        }

        public FormatVersion(string name, string version)
        {
            Name = name;
            var tokens = version.Split('.');
            MajorVersion = int.Parse(tokens[0], CultureInfo.InvariantCulture);
            MinorVersion = tokens.Length > 1 ? int.Parse(tokens[1], CultureInfo.InvariantCulture) : 0;
            BuildVersion = tokens.Length > 2 ? int.Parse(tokens[2], CultureInfo.InvariantCulture) : 0;
        }


        public string Name { get; init; }

        public int MajorVersion { get; init; }
        public int MinorVersion { get; init; }
        public int BuildVersion { get; init; }


        public override string ToString()
        {
            return $"{Name}/{MajorVersion}.{MinorVersion}.{BuildVersion}";
        }

        public static string CreateFormatName(params string[] names)
        {
            return Environment.ApplicationName + "." + string.Join(".", names);
        }

        public static FormatVersion Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return new FormatVersion("", 0, 0, 0);
            }

            var tokens = s.Trim().Split('/');
            var name = tokens[0].Trim();

            if (tokens.Length < 2)
            {
                return new FormatVersion(name, 0, 0, 0);
            }

            var versions = tokens[1].Trim().Split('.');
            var major = (versions.Length > 0) ? int.Parse(versions[0], CultureInfo.InvariantCulture) : 0;
            var minor = (versions.Length > 1) ? int.Parse(versions[1], CultureInfo.InvariantCulture) : 0;
            var build = (versions.Length > 2) ? int.Parse(versions[2], CultureInfo.InvariantCulture) : 0;
            return new FormatVersion(name, major, minor, build);
        }

        public int CompareTo(FormatVersion? other)
        {
            if (other is null) return 1;

            // 名前が一致してなければ比較する意味がない
            Debug.Assert(this.Name == other.Name);

            if (this.Name != other.Name)
            {
                return string.Compare(this.Name, other.Name, StringComparison.InvariantCulture);
            }
            if (this.MajorVersion != other.MajorVersion)
            {
                return this.MajorVersion - other.MajorVersion;
            }
            if (this.MinorVersion != other.MinorVersion)
            {
                return this.MinorVersion - other.MinorVersion;
            }
            if (this.BuildVersion != other.BuildVersion)
            {
                return this.BuildVersion - other.BuildVersion;
            }
            return 0;
        }
    }

    public sealed class JsonFormatVersionConverter : JsonConverter<FormatVersion>
    {
        public override FormatVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) return null;
            return FormatVersion.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, FormatVersion value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }


    public record class VersionNumber(int MajorVersion, int MinorVersion, int BuildVersion)
    {
#if DEBUG
        public static VersionNumber Ver45_Alpha4 { get; } = new(99, 0, 0);
        public static VersionNumber Playlist2_0_1 { get; } = new(99, 0, 0);
#else
        public static VersionNumber Ver45_Alpha4 { get; } = new(45, 0, 3981);
        public static VersionNumber Playlist2_0_1 { get; } = new(2, 0, 1);
#endif
    }
}
