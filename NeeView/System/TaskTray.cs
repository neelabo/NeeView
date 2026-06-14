using H.NotifyIcon;
using System;
using System.Diagnostics;
using System.Threading;

namespace NeeView;

public partial class TaskTray : IDisposable
{
    private readonly static Mutex _mutex;

    private TaskbarIcon? _notifyIcon;
    private bool _disposedValue;

    static TaskTray()
    {
        var notifyIcon = CreateTaskbarIcon();
        var mutexName = "NeeView-" + notifyIcon.TrayIcon.Id.ToString();

        _mutex = new Mutex(false, mutexName);
    }


    public bool IsEnabled
    {
        get { return _notifyIcon is not null; }
        set
        {
            if (value)
            {
                Open();
            }
            else
            {
                Close();
            }
        }
    }


    public void Open()
    {
        if (_disposedValue) return;

        if (_notifyIcon is not null) return;

        var isOwner = false;
        try
        {
            isOwner = _mutex.WaitOne(0);
        }
        catch (AbandonedMutexException ex)
        {
            Debug.WriteLine(ex.Message);
            isOwner = true;
        }

        if (isOwner)
        {
            _notifyIcon = CreateTaskbarIcon();
            _notifyIcon.ForceCreate(enablesEfficiencyMode: false);
        }
    }

    public void Close()
    {
        if (_notifyIcon is null) return;

        _notifyIcon?.Dispose();
        _notifyIcon = null;

        _mutex.ReleaseMutex();
    }

    private static TaskbarIcon CreateTaskbarIcon()
    {
        return (TaskbarIcon)App.Current.FindResource("TaskTrayIcon");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
            }

            _notifyIcon?.Dispose();
            _notifyIcon = null;

            _mutex.Dispose();

            _disposedValue = true;
        }
    }

    ~TaskTray()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

