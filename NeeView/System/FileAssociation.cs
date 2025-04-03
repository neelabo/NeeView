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
            // [HKCU\Software\Classes\.xxx]
            using (var extKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{_extension}", true))
            {
                LocalDebug.WriteLine($"Set registry value: [{extKey}] (default)={_progId}");
                extKey.SetValue("", _progId);
            }

            Debug.WriteLine($"FileAssociate: ON: {Extension}");
        }

        private void Unassociate()
        {
            // No touch [HKCU\Software\Classes\.xxx]
            // https://learn.microsoft.com/en-us/windows/win32/shell/fa-file-types#deleting-registry-information-during-uninstallation

            // Unregister the ProgID
            LocalDebug.WriteLine(@$"Delete registry: [HKEY_CURRENT_USER\Software\Classes\{_progId}]");
            Registry.CurrentUser.DeleteSubKeyTree(@$"Software\Classes\{_progId}");

            // Explorer の UserChoice を削除しないと、例えば ".jpg" の場合は既定のアプリではないマイナーなアプリに切り替わってしまう問題がある。
            // 通常、UserChoice は編集できないようになっており、Microsoft はこれを編集することを拒否しているようだ。
            // このため、複数のプログラムが登録されている拡張子のように、必要な場合に限り慎重に削除するようにしている。
            // TODO: この方法で良いのか？さらなる調査が必要。

            // 1. Check if more than one program is registered
            // [HKCR\Software\Classes\.xxx\OpenWithProgids]
            using (var progIdsKey = Registry.ClassesRoot.OpenSubKey(@$"{_extension}\OpenWithProgids", false))
            {
                if (progIdsKey is not null)
                {
                    var names = progIdsKey.GetValueNames().Where(e => e != _progId).ToList();
                    LocalDebug.WriteLine(@$"Check associate entries: [{progIdsKey}], Count={names.Count}");
                    if (names.Count > 0)
                    {
                        // 2. Remove Explorer UserChoice.
                        RemoveUserChoiceIfProgId(_extension, _progId);
                    }
                }
            }

            Debug.WriteLine($"FileAssociate: OFF: {Extension}");
        }

        /// <summary>
        /// Remove [HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.xxx\UserChoice]
        /// </summary>
        /// <param name="extension">.xxx</param>
        /// <param name="progId">Remove only if the choice is this ProgId</param>
        private static void RemoveUserChoiceIfProgId(string extension, string progId)
        {
            try
            {
                // [HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.xxx]
                using var extKey = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}", true);
                if (extKey is null) return;

                // check UserChoice.ProgID
                using (var userChoiceKey = extKey.OpenSubKey("UserChoice", false))
                {
                    if (userChoiceKey is null) return;
                    var choice = userChoiceKey.GetValue("ProgId") as string;
                    if (choice != progId)
                    {
                        LocalDebug.WriteLine(@$"Cancel remove registry: [{extKey}\UserChoice]={choice}, not {progId}");
                        return;
                    }
                }

                // remove UserChoice 'Deny' permission
                // from https://stackoverflow.com/questions/6108128/remove-a-deny-rule-permission-from-the-userchoice-key-in-the-registry-via
                using (var userChoiceKey = extKey.OpenSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions))
                {
                    if (userChoiceKey == null) return;

                    string userName = WindowsIdentity.GetCurrent().Name;
                    RegistrySecurity security = userChoiceKey.GetAccessControl();

                    AuthorizationRuleCollection accRules = security.GetAccessRules(true, true, typeof(NTAccount));

                    foreach (RegistryAccessRule ar in accRules)
                    {
                        if (ar.IdentityReference.Value == userName && ar.AccessControlType == AccessControlType.Deny)
                        {
                            // remove the 'Deny' permission
                            security.RemoveAccessRuleSpecific(ar);
                        }
                    }

                    // restore all original permissions *except* for the 'Deny' permission
                    userChoiceKey.SetAccessControl(security);
                }

                // remove UserChoice
                LocalDebug.WriteLine(@$"Remove registry: [{extKey}\UserChoice]");
                extKey.DeleteSubKeyTree("UserChoice");
            }
            catch (Exception ex)
            {
                // 削除できなくても致命的な問題ではないので例外は無視する
                Debug.WriteLine(ex.Message);
            }
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
