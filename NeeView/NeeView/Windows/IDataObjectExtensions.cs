using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace NeeView.Windows
{
    public static class IDataObjectExtensions
    {
        private static readonly string _preferredDropEffectFormat = "Preferred DropEffect";

        public static void SetPreferredDropEffect(this IDataObject data, DragDropEffects effect)
        {
            byte[] bytes = [(byte)effect, 0, 0, 0];
            data.SetData(_preferredDropEffectFormat, new System.IO.MemoryStream(bytes));
        }

        public static T? GetData<T>(this IDataObject data)
            where T : class
        {
            try
            {
                return data.GetData(typeof(T)) as T;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T? GetData<T>(this IDataObject data, string format)
            where T : class
        {
            try
            {
                return data.GetData(format) as T;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string[] GetFileDrop(this IDataObject data)
        {
            try
            {
                return (string[])data.GetData(DataFormats.FileDrop, false) ?? [];
            }
            // 応急処置：パスが MAX_PATH を超えていると COMException が発生することがあるので、その場合は空の配列を返す
            catch (COMException ex)
            {
                Debug.WriteLine(ex.Message);
                return [];
            }
        }
    }

}
