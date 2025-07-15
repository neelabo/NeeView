using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace NeeView
{
    public class ScriptManager : IDisposable
    {
        private static ScriptManager? _current;
        public static ScriptManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new ScriptManager(CommandTable.Current);
                    ApplicationDisposer.Current.Add(_current);
                }

                return _current;
            }
        }


        private readonly CommandTable _commandTable;
        private bool _isDirty = true;
        private readonly ScriptUnitPool _pool = new();
        private readonly ScriptFolderWatcher _watcher;
        private bool _disposedValue;
        private readonly ScriptCommandSourceMap _sourceMap = new();
        private readonly DisposableCollection _disposableCollection = new();
        private readonly ScriptEventer _events;

        public ScriptManager(CommandTable commandTable)
        {
            _commandTable = commandTable;

            _events = new ScriptEventer();
            _disposableCollection.Add(_events);

            _watcher = new ScriptFolderWatcher();
            _watcher.Changed += (s, e) => UpdateScriptCommands(true, false);

            _disposableCollection.Add(Config.Current.Script.SubscribePropertyChanged(nameof(ScriptConfig.IsScriptFolderEnabled),
                (s, e) => ScriptConfig_Changed()));

            _disposableCollection.Add(Config.Current.Script.SubscribePropertyChanged(nameof(ScriptConfig.ScriptFolder),
                (s, e) => ScriptConfig_Changed()));

            UpdateWatcher();
        }


        private void ScriptConfig_Changed()
        {
            if (_disposedValue) return;

            UpdateScriptCommands(isForce: true, isReplace: false);
            UpdateWatcher();
        }

        private void UpdateWatcher()
        {
            if (_disposedValue) return;

            if (Config.Current.Script.IsScriptFolderEnabled)
            {
                _watcher.Start(Config.Current.Script.ScriptFolder);
            }
            else
            {
                _watcher.Stop();
            }
        }

        public void OpenScriptsFolder()
        {
            if (_disposedValue) return;

            var path = Config.Current.Script.ScriptFolder;
            if (string.IsNullOrEmpty(path))
            {
                new MessageDialog(TextResources.GetString("OpenScriptsFolderErrorDialog.FolderIsNotSet"), TextResources.GetString("OpenScriptsFolderErrorDialog.Title")).ShowDialog();
                return;
            }

            try
            {
                var directory = new DirectoryInfo(path);
                if (!directory.Exists)
                {
                    directory.Create();
                    ResourceTools.ExportFileFromResource(System.IO.Path.Combine(directory.FullName, "Sample.nvjs"), "/Resources/Scripts/Sample.nvjs");
                    ScriptConfig_Changed();
                }
                ExternalProcess.OpenWithFileManager(path, true, new ExternalProcessOptions() { IsThrowException = true });
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, TextResources.GetString("OpenScriptsFolderErrorDialog.Title")).ShowDialog();
            }
        }


        /// <summary>
        /// コマンドテーブルのスクリプトコマンド更新要求
        /// </summary>
        /// <param name="isForce">強制実行</param>
        /// <param name="isReplace">登録済スクリプトも置き換える</param>
        /// <returns>実行した</returns>
        public bool UpdateScriptCommands(bool isForce, bool isReplace)
        {
            if (_disposedValue) return false;

            if (!isForce && !_isDirty) return false;
            _isDirty = false;

            _sourceMap.Update();

            var commands = _sourceMap.Values
                .Select(e => new ScriptCommand(e.Path, _sourceMap))
                .ToList();

            _commandTable.SetScriptCommands(commands, isReplace);
            return true;
        }


        public void Execute(object? sender, string path, string? name, List<object>? args)
        {
            if (_disposedValue) return;

            _pool.Run(sender, path, name, args);
        }

        public void CancelAll()
        {
            if (_disposedValue) return;

            _pool.CancelAll();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CancelAll();

                    _watcher.Dispose();
                    _disposableCollection.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
