using NeeView.Text;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if DEBUG
namespace NeeView
{
    public static class DebugTest
    {
        public static async Task ExecuteTestAsync()
        {
            ////StringTools.TestStringArgumentSplitter();
            ////return;

            var sw = Stopwatch.StartNew();
            Debug.WriteLine("\n[DebugTest]...");
            await Task.CompletedTask;

            // 致命的エラーのテスト
            InnerExcepionTest.Execute();

            try
            {
                // Archive のキャッシュ一覧
                //ArchiveManager.Current.DumpCache();

                //
                ////new SevenZiPTest().Execute();

                // ArchiveEntry収集テスト
                //await ArchiveEntryCollectionTest.ExecuteAsync(CancellationToken.None);

                // ブックサムネイル作成テスト
                //await DebugCreateBookThumbnail.TestAsync();



                // アーカイブのアンロック
                ////await Task.Run(() => BookOperation.Current.Unlock());

                ////プレイリスト多数登録テスト
                ////BookOperation.Current.Test_MakeManyMarkers();

                // キャッシュ削除(1分前)
                ThumbnailCache.Current.Delete(TimeSpan.FromMinutes(1));


                //Config.Current.RemoveApplicationData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debugger.Break();
            }

            sw.Stop();

            // done.
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();

            Debug.WriteLine($"[DebugTest] done: {sw.ElapsedMilliseconds:#,0}ms");
            ////Debugger.Break();
        }


        static class InnerExcepionTest
        {
            public static void Execute()
            {
                throw new ApplicationException("Exception test", new OperationCanceledException());
            }
        }

        static class ArchiveEntryCollectionTest
        {
            public static async Task ExecuteAsync(CancellationToken token)
            {
                var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip";
                //var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip\サブフォルダテストX";
                //var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip\圧縮再帰♥.zip\root";
                //var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip\圧縮再帰♥.zip\root\dir2?.zip";

                var collection = new ArchiveEntryCollection(path, ArchiveEntryCollectionMode.IncludeSubArchives, ArchiveEntryCollectionMode.IncludeSubArchives, ArchiveEntryCollectionOption.None, ArchiveHint.None);

                Debug.WriteLine($"\n> {collection.Path}");

                var entries = await collection.GetEntriesAsync(token);

                var prefix = LoosePath.TrimDirectoryEnd(collection.Path);
                DumpEntries("Raw", entries, prefix);

                // filter: ページとして画像ファイルのみリストアップ
                var p1 = await collection.GetEntriesWhereImageAsync(token);
                DumpEntries("ImageFilter", p1, prefix);

                // filter: ページとしてすべてのファイルをリストアップ。フォルダーはk空フォルダーのみリストアップ
                var p2 = await collection.GetEntriesWherePageAllAsync(token);
                DumpEntries("AllPageFilter", p2, prefix);

                // filter: アーカイブのみリストアップ。以前の動作
                //var archives = entries.Select(e => e.IsArchive())
            }

            private static void DumpEntries(string label, IEnumerable<ArchiveEntryNode> entries, string prefix)
            {
                Debug.WriteLine($"\n[{label}]");
                foreach (var entry in entries.Select(e => e.ArchiveEntry))
                {
                    var attribute = entry.IsDirectory ? "D" : entry.IsArchive() ? "A" : entry.IsImage() ? "I" : "?";
                    var name = entry.SystemPath[prefix.Length..];
                    Debug.WriteLine(attribute + " " + name);
                }
            }
        }

#if false
        class SevenZiPTest
        {
            private Dictionary<ArchiveFileInfo, MemoryStream> _map = new Dictionary<ArchiveFileInfo, MemoryStream>();

            public void Execute()
            {
                SevenZipArchive.InitializeLibrary();

                var path = @"E:\Work\Labo\サンプル\ソリッド圧縮ON.7z";
                using (var extractor = new SevenZipExtractor(path))
                {
                    // Func<ArchiveFileInfo, Stream> getStreamFunc
                    extractor.FileExtractionFinished += Extractor_FileExtractionFinished;
                    extractor.ExtractArchive(GetStreamFunc);
                }

                foreach (var item in _map)
                {
                    Debug.WriteLine($"{item.Key.Index}: {item.Key.FileName}: {item.Value.Length} ==  {item.Value.Length}");
                    Debug.Assert(item.Value.Length == item.Value.Length);
                }

                Debug.WriteLine("done.");

#region NEXT

                using (var extractor = new SevenZipExtractor(path))
                {
                    var directory = Temporary.Current.CreateCountedTempFileName("arc", "");
                    var temp = new SevenZipTempFileExtractor();
                    temp.TempFileExtractionFinished += Temp_TempFileExtractionFinished;

                    temp.ExtractArchive(extractor, directory);
                }

                void Temp_TempFileExtractionFinished(object? sender, SevenZipTempFileExtractionArgs e)
                {
                    Debug.WriteLine($"{e.FileInfo.Index}: {e.FileName}");
                }

#endregion
            }

            private void Extractor_FileExtractionFinished(object? sender, FileInfoEventArgs e)
            {
                var info = e.FileInfo;
                Debug.WriteLine($"{info.Index}: {info.FileName}");

                try
                {
                    if (_map.TryGetValue(info, out var ms))
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        //ms.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            private Stream GetStreamFunc(ArchiveFileInfo info)
            {
                Debug.WriteLine($"{info.Index}: {info.FileName}");

                var ms = info.IsDirectory ? null : new MemoryStream();
                _map.Add(info, ms);
                return ms;
            }
        }
#endif
    }


}
#endif


