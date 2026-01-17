using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeeView
{
    /// <summary>
    /// JOBスケジューラー
    /// </summary>
    public partial class JobScheduler : BindableBase 
    {
        [Conditional("DEBUG")]
        protected void DebugRaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            RaisePropertyChanged(propertyName);
        }



        public Dictionary<JobClient, List<JobSource>> _clients = new();
        private List<JobSource> _queue = new();


        [Subscribable]
        public event EventHandler? QueueChanged;


        // TODO: Lockオブジェクトはプライベートにしたい
        public System.Threading.Lock Lock { get; } = new();


        public List<JobSource> Queue
        {
            get { return _queue; }
            set
            {
                if (SetProperty(ref _queue, value))
                {
                    DebugRaisePropertyChanged(nameof(JobCount));
                }
            }
        }

        public int JobCount => Queue.Count(e => !e.IsProcessed);


        public void LockAction(Action action)
        {
            lock (Lock)
            {
                action();
            }
        }

        public void RaiseQueueChanged()
        {
            lock (Lock)
            {
                QueueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Client登録
        /// </summary>
        public void RegistClent(JobClient client)
        {
            lock (Lock)
            {
                if (!_clients.ContainsKey(client))
                {
                    _clients.Add(client, new List<JobSource>());
                }
            }
        }

        public void UnregistClient(JobClient client)
        {
            lock (Lock)
            {
                _clients.Remove(client);
            }
        }

        /// <summary>
        /// JOB要求
        /// </summary>
        /// <param name="sender">発行元</param>
        /// <param name="orders">JOBオーダー。並び順がそのまま優先度</param>
        public List<JobSource> Order(JobClient sender, List<JobOrder> orders)
        {
            lock (Lock)
            {
                if (!_clients.ContainsKey(sender)) return new List<JobSource>();

                // TODO: 同じリクエストだったらなにもしない、とか、ここでする？

                // 対象カテゴリの有効JOBの取得
                var collection = Queue.Where(e => e.Category == sender.Category && e.Job.State != JobState.Closed).ToList();

                // オーダーがcollectionにない場合はそのJOBを作成、追加。
                var sources = new List<JobSource>();
                foreach (var order in orders)
                {
                    var source = collection.FirstOrDefault(e => e.Key == order.Key);
                    if (source == null)
                    {
                        source = order.CreateJobSource();
                    }

                    sources.Add(source);
                }

                // そのクライアントのオーダーとして記憶
                Debug.Assert(_clients.ContainsKey(sender));
                _clients[sender] = sources;

                // 新しいQueue
                var queue = _clients.OrderByDescending(e => e.Key.Category.Priority).SelectMany(e => e.Value).ToList();
                ////Debug.WriteLine($"New: {queue.Count}");

                // 管理対象外のJOBにはキャンセル命令発行
                var removes = Queue.Except(queue).ToList();
                foreach (var remove in removes)
                {
                    ////Debug.WriteLine($"JobScheduler.Cancel: {remove}: {remove.GetHashCode()}");
                    remove.Cancel();
                    // TODO: Disposeする？
                }

                // Queue更新
                Queue = queue;
                QueueChanged?.Invoke(this, EventArgs.Empty);

                return sources;
            }
        }

        /// <summary>
        /// 次に処理するJOBを取得
        /// </summary>
        /// <returns></returns>
        public Job? FetchNextJob(int minPriority, int maxPriority)
        {
            lock (Lock)
            {
                var source = Queue.FirstOrDefault(e => !e.IsProcessed && minPriority <= e.Category.Priority && e.Category.Priority <= maxPriority);
                if (source != null)
                {
                    source.IsProcessed = true;
                    ////Debug.WriteLine($"JobScheduler.Processed: {source}");
                    DebugRaisePropertyChanged(nameof(JobCount));
                    return source.Job;
                }

                return null;
            }
        }
    }

}
