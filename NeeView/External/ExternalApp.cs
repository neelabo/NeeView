using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class ExternalApp : BindableBase, ICloneable, IExternalApp
    {
        [DefaultEquality] private string? _name;
        [DefaultEquality] private string? _command;
        [DefaultEquality] private string _parameter = OpenExternalAppCommandParameter.DefaultParameter;
        [DefaultEquality] private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        [DefaultEquality] private string? _workingDirectory;


        // 表示名
        [JsonIgnore]
        public string DisplayName => _name ?? (string.IsNullOrWhiteSpace(_command) ? TextResources.GetString("Word.DefaultApp") : LoosePath.GetFileNameWithoutExtension(_command));

        // 名前
        public string? Name
        {
            get { return _name; }
            set { if (SetProperty(ref _name, string.IsNullOrWhiteSpace(value) ? null : value.Trim())) RaisePropertyChanged(nameof(DisplayName)); }
        }

        // コマンド
        public string? Command
        {
            get { return _command; }
            set { if (SetProperty(ref _command, string.IsNullOrWhiteSpace(value) ? null : value.Trim())) RaisePropertyChanged(nameof(DisplayName)); }
        }

        // コマンドパラメータ
        // {File} = 渡されるファイルパス
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
                new MessageDialog(TextResources.GetString("OpenApplicationErrorDialog.Title"), ex.Message).ShowDialog();
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void ValidatePlaceholder()
        {
            Command = Command is null ? null : StringTemplate.StringFormatTools.ValidatePlaceholder(Command, "NeeView");
            Parameter = StringTemplate.StringFormatTools.ValidatePlaceholder(Parameter, "File");
            Parameter = StringTemplate.StringFormatTools.ValidatePlaceholder(Parameter, "Uri");
        }
    }


}
