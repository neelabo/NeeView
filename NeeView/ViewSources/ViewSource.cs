using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using NeeLaboratory.Threading;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ViewSource : IDataSource, IMemoryElement, IHasImageSource, INotifyPropertyChanged
    {
        private readonly PageContent _pageContent;
        private readonly BookMemoryService _bookMemoryService;
        private PageDataSource _data;
        private readonly AsyncLock _asyncLock = new();
        private IViewSourceStrategy? _strategy;


        public ViewSource(PageContent pageContent, PageDataSource pageDataSource, BookMemoryService bookMemoryService)
        {
            _pageContent = pageContent;
            _bookMemoryService = bookMemoryService;

            _data = new PageDataSource()
            {
                PictureInfo = pageDataSource.PictureInfo,
                Size = pageDataSource.Size
            };
        }


        [Subscribable]
        public event EventHandler<DataSourceChangedEventArgs>? DataSourceChanged;

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;



        public PageDataSource DataSource
        {
            get { return _data; }
            private set
            {
                if (SetProperty(ref _data, value))
                {
                    if (_data.DataSize > 0)
                    {
                        _bookMemoryService.AddPictureSource(this);
                    }
                    DataSourceChanged?.Invoke(this, new DataSourceChangedEventArgs(_data));

                    RaisePropertyChanged(nameof(Data));
                    RaisePropertyChanged(nameof(DataSize));
                    RaisePropertyChanged(nameof(ErrorMessage));
                    RaisePropertyChanged(nameof(IsLoaded));
                    RaisePropertyChanged(nameof(IsFailed));
                }
            }
        }

        public object? Data => _data.Data;
        public long DataSize => _data.DataSize;

        public string? ErrorMessage => _data.ErrorMessage;
        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;

        public IMemoryOwner Owner => _pageContent;
        public long MemorySize => DataSize;

        public ImageSource? ImageSource => (Data as ImageSource) ?? (Data as IHasImageSource)?.ImageSource;


        /// <summary>
        /// 求める ViewSource データ生成済かをチェック
        /// </summary>
        /// <param name="size">サイズ</param>
        /// <returns>生成済？</returns>
        private bool CheckLoaded(Size size)
        {
            return IsLoaded && _strategy?.CheckLoaded(size) == true;
        }

        /// <summary>
        /// ViewSource データ生成
        /// </summary>
        /// <param name="size">サイズ</param>
        /// <param name="token">キャンセルトークン</param>
        public async Task LoadAsync(Size size, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using (await _asyncLock.LockAsync(token))
            {
                // ロード済であれば何もしない
                if (CheckLoaded(size))
                {
                    return;
                }

                // データ読み込み
                PageDataSource data;
                try
                {
                    data = await _pageContent.LoadAsync(token);
                }
                catch (Exception ex)
                {
                    SetData(null, 0, ex.Message, null, PageContent.DefaultSize);
                    return;
                }

                if (data.IsFailed)
                {
                    SetData(null, 0, data.ErrorMessage, data.PictureInfo, data.Size);
                    return;
                }

                // NOTE: エラーなく読み込んだのにデータが無いとかありえないが念のため
                if (data.Data is null)
                {
                    SetData(null, 0, "InvalidOperationException: cannot load data", data.PictureInfo, data.Size);
                    return;
                }

                try
                {
                    await LoadCoreAsync(data, size, token);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    SetData(null, 0, ex.Message, data.PictureInfo, data.Size);
                }
            }
        }

        private async Task LoadCoreAsync(PageDataSource data, Size size, CancellationToken token)
        {
            NVDebug.AssertMTA();

            if (data.IsFailed)
            {
                SetData(null, 0, data.ErrorMessage, data.PictureInfo, data.Size);
            }
            else
            {
                if (_strategy is null)
                {
                    _strategy = ViewSourceStrategyFactory.Create(_pageContent, data);
                }

                if (_strategy is not null)
                {
                    SetData(await _strategy.LoadCoreAsync(data, size, token), data.PictureInfo, data.Size);
                }
                else
                {
                    SetData(null, 0, $"InvalidOperationException: No ViewSourceStrategy", data.PictureInfo, data.Size);
                }
            }
        }

        public void Unload()
        {
            if (_data.Data is not null)
            {
                _strategy?.Unload();
                SetData(null, 0, null, _data.PictureInfo, _data.Size);
            }
        }


        private void SetData(object? data, long dataSize, string? errorMessage, PictureInfo? pictureInfo, Size size)
        {
            var pageDataSource = new PageDataSource()
            {
                Data = data,
                DataSize = dataSize,
                ErrorMessage = errorMessage,
                PictureInfo = pictureInfo,
                Size = size
            };
            DataSource = pageDataSource;
        }

        private void SetData(DataSource data, PictureInfo? pictureInfo, Size size)
        {
            var pageDataSource = new PageDataSource()
            {
                Data = data.Data,
                DataSize = data.DataSize,
                ErrorMessage = data.ErrorMessage,
                PictureInfo = pictureInfo,
                Size = size
            };
            DataSource = pageDataSource;
        }
    }

}
