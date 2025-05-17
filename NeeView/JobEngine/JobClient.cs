using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// JOBの発行者。
    /// </summary>
    public class JobClient
    {
        public JobClient(string name, JobCategory category)
        {
            Debug.Assert(category != null);
            Name = name;
            Category = category;
        }

        public string Name { get; set; }

        public JobCategory Category { get; private set; }

        public override string? ToString()
        {
            return Name ?? base.ToString();
        }
    }


    /// <summary>
    /// ページコンテンツ JobClient
    /// </summary>
    public class PageContentJobClient : JobClient, IDisposable
    {
        private List<JobSource> _sources = new();

        public PageContentJobClient(string name, JobCategory category) : base(name, category)
        {
            Name = name;
            JobEngine.Current.RegistClient(this);
        }

        /// <summary>
        /// JOB要求
        /// </summary>
        /// <param name="pages">読み込むページ</param>
        public void Order(List<IPageContentLoader> pages)
        {
            if (_disposedValue) return;

            var orders = pages
                .Where(e => !e.IsLoaded)
                .Select(e => new JobOrder(this.Category, e))
                .ToList();

            _sources = JobEngine.Current.Order(this, orders);
        }


        /// <summary>
        /// JOBの完了を待つ (すべて)
        /// </summary>
        public async ValueTask WaitAsync(CancellationToken token)
        {
            await Task.WhenAll(_sources.Select(e => e.WaitAsync(token).AsTask()));
        }

        /// <summary>
        /// JOBの完了を待つ
        /// </summary>
        /// <param name="pages">完了待ちをするページ</param>
        public async ValueTask WaitAsync(List<IPageContentLoader> pages, int millisecondsTimeout, CancellationToken token)
        {
            if (_disposedValue) return;

            var tasks = pages
                .Select(e => (_sources.FirstOrDefault(a => a.Key == e)?.WaitAsync(millisecondsTimeout, token).AsTask()))
                .WhereNotNull()
                .ToList();

            await Task.WhenAll(tasks);
        }


        public void CancelOrder()
        {
            if (_disposedValue) return;

            JobEngine.Current.CancelOrder(this);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    JobEngine.Current.UnregistClient(this);
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }


    /// <summary>
    /// ページサムネイル JobClient
    /// </summary>
    public class PageThumbnailJobClient : JobClient, IDisposable
    {
        public PageThumbnailJobClient(string name, JobCategory category) : base(name, category)
        {
            Name = name;
            JobEngine.Current.RegistClient(this);
        }

        /// <summary>
        /// JOB要求
        /// </summary>
        /// <param name="pages">読み込むページ</param>
        public void Order(List<IPageThumbnailLoader> pages)
        {
            if (_disposedValue) return;

            var orders = pages
                .Where(e => e != null && !e.IsThumbnailValid)
                .Select(e => new JobOrder(this.Category, e))
                .ToList();

            JobEngine.Current.Order(this, orders);
        }

        public void CancelOrder()
        {
            if (_disposedValue) return;

            JobEngine.Current.CancelOrder(this);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    JobEngine.Current.UnregistClient(this);
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }


}
