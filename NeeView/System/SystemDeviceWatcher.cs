//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeView.Win32.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using ITEMIDLIST = Windows.Win32.UI.Shell.Common.ITEMIDLIST;
using SHChangeNotifyEntry = Windows.Win32.UI.Shell.SHChangeNotifyEntry;

namespace NeeView
{
    public class DriveChangedEventArgs : EventArgs
    {
        public DriveChangedEventArgs(string driveName, bool isAlive)
        {
            Name = driveName;
            IsAlive = isAlive;
        }

        public string Name { get; set; }
        public bool IsAlive { get; set; }

        public override string ToString()
        {
            return $"Name={Name}, IsAlive={IsAlive}";
        }
    }

    public class MediaChangedEventArgs : EventArgs
    {
        public MediaChangedEventArgs(string driveName, bool isAlive)
        {
            Name = driveName;
            IsAlive = isAlive;
        }

        public string Name { get; set; }
        public bool IsAlive { get; set; }

        public override string ToString()
        {
            return $"Name={Name}, IsAlive={IsAlive}";
        }
    }

    public enum DirectoryChangeType
    {
        Created = 1,
        Deleted = 2,
        Changed = 4,
        Renamed = 8,
        All = 15
    }

    public class DirectoryChangedEventArgs : EventArgs
    {
        public DirectoryChangedEventArgs(DirectoryChangeType changeType, string fullPath, string? oldFullPath)
        {
            if (changeType == DirectoryChangeType.All) throw new ArgumentOutOfRangeException(nameof(changeType));

            ChangeType = changeType;
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));

            if (changeType == DirectoryChangeType.Renamed)
            {
                OldFullPath = oldFullPath ?? throw new ArgumentNullException(nameof(oldFullPath));

                if (Path.GetDirectoryName(OldFullPath) != Path.GetDirectoryName(FullPath))
                {
                    throw new ArgumentException("Not same directory");
                }
            }
        }

        public DirectoryChangedEventArgs(DirectoryChangeType changeType, string fullPath) : this(changeType, fullPath, null)
        {
            if (changeType == DirectoryChangeType.Renamed) throw new InvalidOperationException();
        }


        public DirectoryChangeType ChangeType { get; set; }
        public string FullPath { get; set; }
        public string? OldFullPath { get; set; }

        public override string ToString()
        {
            return $"ChangeType={ChangeType}, FullPath={FullPath}, OldFullPath={OldFullPath}";
        }
    }

    public class SettingChangedEventArgs : EventArgs
    {
        public SettingChangedEventArgs(uint action, string? message)
        {
            Action = action;
            Message = message;
        }

        public uint Action { get; private set; }
        public string? Message { get; private set; }

        public override string ToString()
        {
            return $"Action={Action}, Message={Message}";
        }
    }


    [LocalDebug]
    public partial class SystemDeviceWatcher
    {
        static SystemDeviceWatcher() => Current = new SystemDeviceWatcher();
        public static SystemDeviceWatcher Current { get; }


        private Window? _window;

        public SystemDeviceWatcher()
        {
        }


        [Subscribable]
        public event EventHandler<DriveChangedEventArgs>? DriveChanged;

        [Subscribable]
        public event EventHandler<MediaChangedEventArgs>? MediaChanged;

        [Subscribable]
        public event EventHandler<DirectoryChangedEventArgs>? DirectoryChanged;

        [Subscribable]
        public event EventHandler<SettingChangedEventArgs>? SettingChanged;

        [Subscribable]
        public event EventHandler? EnterSizeMove;

        [Subscribable]
        public event EventHandler? ExitSizeMove;


        // ウィンドウプロシージャ初期化
        public void Initialize(Window window)
        {
            if (_window != null) throw new InvalidOperationException();

            var hWndSrc = HwndSource.FromVisual(window) as HwndSource ?? throw new InvalidOperationException("Cannot get window handle");

            _window = window;

            var notifyEntry = new SHChangeNotifyEntry() { pidl = null, fRecursive = true };
            var notifyId = PInvoke.SHChangeNotifyRegister((HWND)hWndSrc.Handle,
                                                  SHCNRF_SOURCE.SHCNRF_ShellLevel,
                                                  (int)(SHCNE_ID.SHCNE_MEDIAINSERTED | SHCNE_ID.SHCNE_MEDIAREMOVED | SHCNE_ID.SHCNE_MKDIR | SHCNE_ID.SHCNE_RMDIR | SHCNE_ID.SHCNE_RENAMEFOLDER),
                                                  WindowMessages.WM_SHNOTIFY,
                                                  1,
                                                  notifyEntry);

            hWndSrc.AddHook(WndProc);
        }


        // ウィンドウプロシージャ
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                switch ((uint)msg)
                {
                    case PInvoke.WM_ENTERSIZEMOVE:
                        EnterSizeMove?.Invoke(this, EventArgs.Empty);
                        break;
                    case PInvoke.WM_EXITSIZEMOVE:
                        ExitSizeMove?.Invoke(this, EventArgs.Empty);
                        break;
                    case PInvoke.WM_DEVICECHANGE:
                        OnDeviceChange(wParam, lParam);
                        break;
                    case PInvoke.WM_SETTINGCHANGE:
                        OnSettingChange(wParam, lParam);
                        break;
                    case WindowMessages.WM_SHNOTIFY:
                        OnSHNotify(wParam, lParam);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// 設定変更イベント
        /// </summary>
        private void OnSettingChange(IntPtr wParam, IntPtr lParam)
        {
            var action = (uint)wParam;
            string? str = Marshal.PtrToStringAuto(lParam);
            ////Trace.WriteLine($"WM_SETTINGCHANGE: {action:X4}, {str}");

            SettingChanged?.Invoke(this, new SettingChangedEventArgs(action, str));
        }

        private void OnDeviceChange(IntPtr wParam, IntPtr lParam)
        {
            if (lParam == IntPtr.Zero)
            {
                return;
            }

            var volume = (DEV_BROADCAST_VOLUME?)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_VOLUME));
            if (volume is null) return;

            var driveName = UnitMaskToDriveName(volume.Value.dbcv_unitmask);
            if (driveName == null)
            {
                return;
            }

            switch ((uint)wParam)
            {
                case PInvoke.DBT_DEVICEARRIVAL:
                    ////Debug.WriteLine("DBT_DEVICEARRIVAL");
                    DriveChanged?.Invoke(this, new DriveChangedEventArgs(driveName, true));
                    break;
                case PInvoke.DBT_DEVICEREMOVECOMPLETE:
                    ////Debug.WriteLine("DBT_DEVICEREMOVECOMPLETE");
                    DriveChanged?.Invoke(this, new DriveChangedEventArgs(driveName, false));
                    break;
            }
        }

        private static string? UnitMaskToDriveName(uint unitMask)
        {
            for (int i = 0; i < 32; ++i)
            {
                if ((unitMask >> i & 1) == 1)
                {
                    return ((char)('A' + i)).ToString() + ":\\";
                }
            }

            return null;
        }

        // TODO:全てのフォルダーの変更が通知される。これは必要な機能としては過剰すぎる。本当に必要か再検討せよ。
        // TODO: 重い処理が多いので、集積かBeginInvokeかする。
        private void OnSHNotify(IntPtr wParam, IntPtr lParam)
        {
            var shNotify = (SHNOTIFYSTRUCT)Marshal.PtrToStructure(wParam, typeof(SHNOTIFYSTRUCT))!;

            var shChangeNotifyEvent = (SHCNE_ID)lParam;

            LocalDebug.WriteLine(shChangeNotifyEvent + ": " + shNotify);

            switch (shChangeNotifyEvent)
            {
                case SHCNE_ID.SHCNE_MEDIAINSERTED:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (path is not null)
                        {
                            var args = new MediaChangedEventArgs(path, true);
                            LocalDebug.WriteLine($"MediaChanged: {args}");
                            MediaChanged?.Invoke(this, args);
                        }
                    }
                    break;
                case SHCNE_ID.SHCNE_MEDIAREMOVED:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (path is not null)
                        {
                            var args = new MediaChangedEventArgs(path, false);
                            LocalDebug.WriteLine($"MediaChanged: {args}");
                            MediaChanged?.Invoke(this, args);
                        }
                    }
                    break;

                case SHCNE_ID.SHCNE_MKDIR:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (!string.IsNullOrEmpty(path))
                        {
                            var args = new DirectoryChangedEventArgs(DirectoryChangeType.Created, path);
                            LocalDebug.WriteLine($"DirectoryChanged: {args}");
                            DirectoryChanged?.Invoke(this, args);
                        }
                    }
                    break;

                case SHCNE_ID.SHCNE_RMDIR:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (!string.IsNullOrEmpty(path))
                        {
                            var args = new DirectoryChangedEventArgs(DirectoryChangeType.Deleted, path);
                            LocalDebug.WriteLine($"DirectoryChanged: {args}");
                            DirectoryChanged?.Invoke(this, args);
                        }
                    }
                    break;

                case SHCNE_ID.SHCNE_RENAMEFOLDER:
                    {
                        var path1 = PIDLToString(shNotify.dwItem1);
                        var path2 = PIDLToString(shNotify.dwItem2);
                        if (!string.IsNullOrEmpty(path1) && !string.IsNullOrEmpty(path2) && path1 != path2)
                        {
                            // path1 is new, path2 is old, maybe.
                            var args = new DirectoryChangedEventArgs(DirectoryChangeType.Renamed, path2, path1);
                            LocalDebug.WriteLine($"DirectoryChanged: {args}");
                            DirectoryChanged?.Invoke(this, args);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private static unsafe string? PIDLToString(IntPtr dwItem)
        {
            if (dwItem == IntPtr.Zero) return null;

            PWSTR psz;
            PInvoke.SHGetNameFromIDList((ITEMIDLIST*)dwItem, SIGDN.SIGDN_FILESYSPATH, &psz);
            string path = psz.ToString();
            PInvoke.CoTaskMemFree(psz);
            return path;
        }
    }
}
