using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Jobカテゴリ
    /// </summary>
    public abstract class JobCategory
    {
        public JobCategory(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; private set; }

        /// <summary>
        /// JOB生成
        /// </summary>
        public abstract Job CreateJob(object key, CancellationToken token);
    }


    /// <summary>
    /// ページコンテンツ作成用 JobCategory
    /// </summary>
    public class PageContentJobCategory : JobCategory
    {
        public PageContentJobCategory(int priority) : base(priority)
        {
        }

        private class JobCommand : IJobCommand
        {
            readonly IPageContentLoader _page;

            public JobCommand(IPageContentLoader page)
            {
                _page = page;
            }

            public void Execute(CancellationToken token)
            {
                _page.LoadContentAsync(token).AsTask().Wait(token);
            }
        }

        public override Job CreateJob(object key, CancellationToken token)
        {
            var page = (IPageContentLoader)key;

            var job = Job.Create(new JobCommand(page), token);
            return job;
        }
    }


    /// <summary>
    /// サムネイル生成用 JobCategory
    /// </summary>
    public class PageThumbnailJobCategory : JobCategory
    {
        public PageThumbnailJobCategory(int priority) : base(priority)
        {
        }

        private class JobCommand : IJobCommand
        {
            readonly IPageThumbnailLoader _page;

            public JobCommand(IPageThumbnailLoader page)
            {
                _page = page;
            }

            public void Execute(CancellationToken token)
            {
                _page.LoadThumbnailAsync(token).AsTask().Wait(token);
            }
        }

        public override Job CreateJob(object key, CancellationToken token)
        {
            var page = (IPageThumbnailLoader)key;

            var job = Job.Create(new JobCommand(page), token);
            return job;
        }
    }



    /// <summary>
    /// JobCategoryインスタンス(固定)
    /// </summary>
    public static class JobCategories
    {
        static JobCategories()
        {
            PageViewContentJobCategory = new PageContentJobCategory(10);
            PageAheadContentJobCategory = new PageContentJobCategory(8);
            PageThumbnailCategory = new PageThumbnailJobCategory(5);
            BookThumbnailCategory = new PageThumbnailJobCategory(0);
        }

        public static PageContentJobCategory PageViewContentJobCategory { get; }
        public static PageContentJobCategory PageAheadContentJobCategory { get; }
        public static PageThumbnailJobCategory PageThumbnailCategory { get; }
        public static PageThumbnailJobCategory BookThumbnailCategory { get; }
    }
}
