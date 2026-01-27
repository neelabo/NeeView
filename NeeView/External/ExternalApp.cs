using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ExternalApp : BindableBase, ICloneable, IExternalApp
    {
        private string? _name;
        private string? _command;
        private string _parameter = OpenExternalAppCommandParameter.DefaultParameter;
        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private string? _workingDirectory;


        // 表示名
        [JsonIgnore]
        public string DisplayName => _name ?? (string.IsNullOrWhiteSpace(_command) ? TextResources.GetString("Word.DefaultApp") : LoosePath.GetFileNameWithoutExtension(_command));

        // 名前
        public string? Name
        {
            get { return _name; }
            set { if (SetProperty(ref _name, value)) RaisePropertyChanged(nameof(DisplayName)); }
        }

        // コマンド
        public string? Command
        {
            get { return _command; }
            set { if (SetProperty(ref _command, value?.Trim())) RaisePropertyChanged(nameof(DisplayName)); }
        }

        // コマンドパラメータ
        // $FILE = 渡されるファイルパス
        public string Parameter
        {
            get { return _parameter; }
            set { SetProperty(ref _parameter, string.IsNullOrWhiteSpace(value) ? OpenExternalAppCommandParameter.DefaultParameter : value); }
        }

        // 作業フォルダー
        public string? WorkingDirectory
        {
            get { return _workingDirectory; }
            set { SetProperty(ref _workingDirectory, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        // 圧縮ファイルのときの動作
        public ArchivePolicy ArchivePolicy
        {
            get { return _archivePolicy; }
            set { SetProperty(ref _archivePolicy, value); }
        }


        public async ValueTask ExecuteAsync(IEnumerable<Page> pages, CancellationToken token)
        {
            var external = new ExternalAppUtility();
            await external.CallAsync(pages, this, token);
        }

        public void Execute(IEnumerable<string> files)
        {
            var external = new ExternalAppUtility();
            try
            {
                external.Call(files, this);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, TextResources.GetString("OpenApplicationErrorDialog.Title")).ShowDialog();
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }


}
