using System.IO;
using System.Threading.Tasks;

namespace NeeLaboratory.IO
{
    public static class FileTools
    {
        public static byte[] ReadAllBytes(string path, FileShare share)
        {
            byte[] result;
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, share))
            {
                result = new byte[stream.Length];
                stream.SafeRead(result, 0, (int)stream.Length);
            }
            return result;
        }

        public static async ValueTask<byte[]> ReadAllBytesAsync(string path, FileShare share)
        {
            byte[] result;
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, share))
            {
                result = new byte[stream.Length];
                await stream.SafeReadAsync(result, 0, (int)stream.Length);
            }
            return result;
        }

        public static async ValueTask WriteAllBytesAsync(string path, byte[] bytes)
        {
            using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                await stream.WriteAsync(bytes);
            }
        }

        /// <summary>
        /// ファイルに上書き
        /// </summary>
        /// <remarks>
        /// Hidden ファイルに対しては WriteAllBytes が失敗することがあるのでその代わりに使用する
        /// </remarks>
        public static void TruncateAllBytes(string path, byte[] bytes)
        {
            using (var fs = new FileStream(path, FileMode.Truncate, FileAccess.Write))
            {
                fs.Write(bytes, 0, bytes.Length);
            }
        }
    }

}
