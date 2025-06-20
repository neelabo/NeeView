using System;
using System.Diagnostics;
using System.IO;


namespace NeeView
{
    /// <summary>
    /// UserSetting ファイルリソース
    /// </summary>
    /// <remarks>
    /// 起動時の UserSetting 読み込み専用
    /// </remarks>
    public class UserSettingResource
    {
        private readonly FileResourceWithBackup _resource;

        public UserSettingResource(string? path)
        {
            _resource = new FileResourceWithBackup(path);
        }

        /// <summary>
        /// バイト列を取得
        /// </summary>
        /// <returns></returns>
        public byte[]? LoadBytes()
        {
            return _resource.GetValidResource()?.Bytes;
        }

        /// <summary>
        /// 起動に必要な最低限の設定を取得
        /// </summary>
        /// <returns></returns>
        public BootSetting? LoadBootSetting()
        {
            var bytes = LoadBytes();
            if (bytes is null) return null;
            return UserSettingTools.LoadBootSetting(bytes);
        }

        /// <summary>
        /// UserSetting を取得
        /// </summary>
        /// <returns>UserSetting ファイルが存在しないときは null、読み込みエラーでは例外が発生する</returns>
        public UserSetting? Load()
        {
            while (true)
            {
                var res = _resource.GetValidResource();
                if (res is null)
                {
                    var ex = _resource.GetException();
                    if (ex is not null) throw ex;
                    return null;
                }

                try
                {
                    Debug.Assert(res.Bytes != null);
                    using var ms = new MemoryStream(res.Bytes);
                    var setting = UserSettingTools.Load(ms);
                    setting?.SetFileStamp(res.FileStamp);
                    // バックアップファイルからの読み込みの場合はバックアップの生成を抑制する
                    if (res.IsBackup)
                    {
                        Debug.WriteLine("Load from backup file.");
                        SaveData.Current.BackupOnce = false;
                    }
                    return setting;
                }
                catch (Exception ex)
                {
                    res.SetException(ex);
                }
            }
        }
    }
}
