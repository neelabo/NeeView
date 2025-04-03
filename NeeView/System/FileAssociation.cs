//#define LOCAL_DEBUG

using Microsoft.Win32;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace NeeView
{
    [LocalDebug]
    public partial class FileAssociation : IFileAssociation
    {
        public const string ProgIdPrefix = "NeeView";
        private readonly string _extension;
        private readonly FileAssociationCategory _category;
        private readonly string _progId;
        private bool _isEnabled;

        public FileAssociation(string extension, FileAssociationCategory category)
        {
            if (string.IsNullOrWhiteSpace(extension)) throw new ArgumentNullException(nameof(extension));
            if (extension[0] != '.') throw new ArgumentException($"{nameof(extension)} does not begin with a period.");

            _extension = extension;
            _category = category;

            // ProgID is "NeeView.[ext]"
            _progId = ProgIdPrefix + _extension;

            _isEnabled = IsAssociated();
        }


        public string Extension => _extension;

        public FileAssociationCategory Category => _category;

        public string? Description { get; init; }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    UpdateAssociate();
                }
            }
        }


        private void UpdateAssociate()
        {
            if (_isEnabled)
            {
                try
                {
                    Associate();
                }
                catch (Exception ex)
                {
                    throw new FileAssociationException($"Cannot associate: {_extension}", ex);
                }
            }
            else
            {
                try
                {
                    Unassociate();
                }
                catch (Exception ex)
                {
                    throw new FileAssociationException($"Cannot unassociate: {_extension}", ex);
                }
            }
        }

        private static bool IsExistSubKey(RegistryKey reg, string name)
        {
            try
            {
                using var key = reg.OpenSubKey(name, false);
                return key != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public bool IsAssociated()
        {
            using (var classesKey = Registry.CurrentUser.CreateSubKey(@$"Software\Classes"))
            {
                if (classesKey is null) return false;
                return IsExistSubKey(classesKey, _progId);
            }
        }

        private void Associate()
        {
            var applicationFilePath = Environment.AssemblyLocation;
            var fileIconPath = Category.ToIconPath();

            // Register the ProgID
            // [HKCU\Software\Classes\NeeView.xxx]
            // [HKCU\Software\Classes\NeeView.xxx\DefaultIcon]
            // [HKCU\Software\Classes\NeeView.xxx\Shell\open\command]
            // [HKCU\Software\Classes\NeeView.xxx\Category] .. for NeeView
            using (var prog = Registry.CurrentUser.CreateSubKey(@$"Software\Classes\{_progId}", true))
            {
                LocalDebug.WriteLine($"Create registry: [{prog}]");
                if (Description is not null)
                {
                    prog.SetValue("", Description);
                }
                prog.SetValue("Category", _category.ToString());
                prog.CreateSubKey(@"DefaultIcon").SetValue("", $"{fileIconPath}");
                prog.CreateSubKey(@"Shell\open\command").SetValue("", $"\"{applicationFilePath}\" \"%1\"");
            }

            // Register the file name extension for the file type
            // [HKCU\Software\Classes\.xxx\OpenWithProgids]
            using (var extKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{_extension}\OpenWithProgids", true))
            {
                LocalDebug.WriteLine($"Set registry value: [{extKey}] {_progId}=[]");
                extKey.SetValue(_progId, "");
            }

            Debug.WriteLine($"FileAssociate: ON: {Extension}");
        }

        private void Unassociate()
        {
            // Unregister the file name extension for the file type
            // [HKCU\Software\Classes\.xxx\OpenWithProgids]
            using (var extKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{_extension}\OpenWithProgids", true))
            {
                LocalDebug.WriteLine($"Delete registry value: [{extKey}] {_progId}");
                extKey.DeleteValue(_progId, false);
            }

            // Unregister the ProgID
            LocalDebug.WriteLine(@$"Delete registry: [HKEY_CURRENT_USER\Software\Classes\{_progId}]");
            Registry.CurrentUser.DeleteSubKeyTree(@$"Software\Classes\{_progId}");

            Debug.WriteLine($"FileAssociate: OFF: {Extension}");
        }
    }


    public class FileAssociationException : Exception
    {
        public FileAssociationException()
        {
        }

        public FileAssociationException(string? message) : base(message)
        {
        }

        public FileAssociationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
