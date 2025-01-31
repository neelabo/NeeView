using System.IO;
using System.Threading.Tasks;
using System;

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

        public static async Task<byte[]> ReadAllBytesAsync(string path, FileShare share)
        {
            byte[] result;
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, share))
            {
                result = new byte[stream.Length];
                await stream.SafeReadAsync(result, 0, (int)stream.Length);
            }
            return result;
        }

        public static async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                await stream.WriteAsync(bytes);
            }
        }
    }

}
