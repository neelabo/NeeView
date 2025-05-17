using NeeLaboratory.IO.Nodes;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Xunit.Abstractions;

namespace NeeLaboratory.IO.NodesTest
{
    public class FileTreeTest
    {
#pragma warning disable CS0414 // フィールドが割り当てられていますが、値は使用されていません
        private static string RootPath { get; } = @"E:\Work\Labo\サンプル\フォルダー、サブフォルダーx3";

        private static readonly string _folderSource = @"TestFolders";
        private static readonly string _folderRoot = @"_Work";
        private static readonly string _folderRoot2 = @"_Work2";

        private static readonly string _folderSub1 = @"_Work\SubFolder1";
        private static readonly string _folderSub2 = @"_Work\SubFolder2";
        private static readonly string _folderSub3 = @"_Work\SubFolder3";
        private static readonly string _folderSub3Ex = @"_Work\SubFolder2\SubFolder3";

        private static readonly string _fileAppend1 = @"_Work\SubFolder1\append1.txt";
        private static readonly string _fileAppend2 = @"_Work\SubFolder1\append2.bin";
        private static readonly string _fileAppend2Ex = @"_Work\SubFolder1\append2.txt";
#pragma warning restore CS0414 // フィールドが割り当てられていますが、値は使用されていません



        private readonly ITestOutputHelper output;

        public FileTreeTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// テスト環境初期化
        /// </summary>
        public static async ValueTask<FileTree> CreateTestEnvironmentAsync(CancellationToken token)
        {
            if (Directory.Exists(_folderRoot)) Directory.Delete(_folderRoot, true);
            if (Directory.Exists(_folderRoot2)) Directory.Delete(_folderRoot2, true);
            CopyDirectory(_folderSource, _folderRoot, true);

            var tree = new FileTree(Path.GetFullPath(_folderRoot), IOExtensions.CreateEnumerationOptions(true, FileAttributes.None));
            await tree.InitializeAsync(token);
            return tree;
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private async Task DumpTree(FileTree tree)
        {
            using (await tree.LockAsync(CancellationToken.None))
            {
                foreach (var (value, index) in tree.WalkAll().Select((value, index) => (value, index)))
                {
                    output.WriteLine($"{index}: {value.FullName})");
                }
            }
        }

        [Fact]
        public async Task FileTreeBasicTest()
        {
            var tree = new FileTree(RootPath, IOExtensions.CreateEnumerationOptions(true, FileAttributes.None));

            await DumpTree(tree);
        }



        /// <summary>
        /// ファイルシステム監視テスト
        /// </summary>
        [Fact]
        public async Task FileTreeWatchFilesystemTest()
        {
            var tree = await CreateTestEnvironmentAsync(CancellationToken.None);

            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(12, tree.WalkAll().Count());
            }

            // ファイル追加 ...
            using (FileStream stream = File.Create(_fileAppend1)) { }
            using (FileStream stream = File.Create(_fileAppend2)) { }
            await Task.Delay(100);

            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(14, tree.WalkAll().Count());
            }

            // 名前変更
            var fileAppend2Ex = Path.ChangeExtension(_fileAppend2, ".txt");
            File.Move(_fileAppend2, fileAppend2Ex);
            await Task.Delay(100);

            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(14, tree.WalkAll().Count());
            }

            // 内容変更
            using (FileStream stream = File.Open(fileAppend2Ex, FileMode.Append))
            {
                stream.WriteByte(0x00);
            }
            await Task.Delay(100);

            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(14, tree.WalkAll().Count());
            }

            // ファイル削除...
            File.Delete(_fileAppend1);
            File.Delete(fileAppend2Ex);
            await Task.Delay(100);

            await DumpTree(tree);

            // 戻ったカウント確認
            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(12, tree.WalkAll().Count());
            }
        }

        // 移動テスト
        [Fact]
        public async Task FileTreeMoveDirectoryTest()
        {
            var tree = await CreateTestEnvironmentAsync(CancellationToken.None);
            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(12, tree.WalkAll().Count());
            }

            Directory.Move(_folderSub1, _folderSub3);
            await Task.Delay(100);
            Assert.False(Directory.Exists(_folderSub1));
            Assert.True(Directory.Exists(_folderSub3));

            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(12, tree.WalkAll().Count());
                var node = tree.Find(Path.GetFullPath(_folderSub3));
                Assert.NotNull(node);
                output.WriteLine($"_folderSub3: {node.FullName}");
            }

            var src = Path.GetFullPath(_folderSub3);
            var dst = Path.GetFullPath(_folderSub3Ex);
            output.WriteLine($"move {src} -> {dst}");
            Directory.Move(src, dst);
            await Task.Delay(100);
            Assert.False(Directory.Exists(_folderSub3));
            Assert.True(Directory.Exists(_folderSub3Ex));

            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(12, tree.WalkAll().Count());
                var node = tree.Find(Path.GetFullPath(_folderSub3Ex));
                Assert.NotNull(node);
                output.WriteLine($"_folderSub3Ex: {node.FullName}");
            }

            await DumpTree(tree);
        }


        /// <summary>
        /// ディレクトリ削除監視
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FileTreeDeleteDirectoryTest()
        {
            var tree = await CreateTestEnvironmentAsync(CancellationToken.None);
            await DumpTree(tree);
            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(12, tree.WalkAll().Count());
            }

            // Sub1フォルダ削除
            Directory.Delete(_folderSub1, true);
            await Task.Delay(100);
            Assert.False(Directory.Exists(_folderSub1));

            using (await tree.LockAsync(CancellationToken.None))
            {
                Assert.Equal(8, tree.WalkAll().Count());
            }

            // 対象そのものが変化 ... は監視対象外なので変化なくテスト対象外

            await DumpTree(tree);
        }
    }
}