using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NeeView
{
    public class ScriptCommandSourceMap : Dictionary<string, ScriptCommandSource>
    {
        public void Update()
        {
            this.Clear();

            if (Config.Current.Script.IsScriptFolderEnabled)
            {
                foreach (var fileInfo in CollectScripts())
                {
                    try
                    {
                        var path = fileInfo.FullName;
                        this.Add(path, ScriptCommandSource.Create(path));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        public static List<FileInfo> CollectScripts()
        {
            if (!string.IsNullOrEmpty(Config.Current.Script.ScriptFolder))
            {
                try
                {
                    var directory = new DirectoryInfo(Config.Current.Script.ScriptFolder);
                    if (FileIO.Exists(directory))
                    {
                        return directory.GetFiles("*" + ScriptCommandSource.Extension).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return new List<FileInfo>();
        }
    }
}
